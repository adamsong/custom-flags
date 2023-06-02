using System.Collections.Generic;
using System.IO;
using BepInEx;
using KSP.Game;
using SpaceWarp;
using SpaceWarp.API.Mods;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CustomFlags
{

	public class FlagResource
	{
		public Texture2D Texture;
		public Sprite Sprite;
	}
	
	[BepInPlugin(ModGuid, ModName, ModVer)]
	[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
	public class CustomFlagsMod : BaseSpaceWarpPlugin
	{
		public const string ModGuid = "us.adamsogm.CustomFlags";
		public const string ModVer = "3.0.1";
		public const string ModName = "CustomFlags";
		public static string Path { get; private set; }

		public override void OnPreInitialized()
		{
			Path = PluginFolderPath;
		}

		internal static readonly Dictionary<string, FlagResource> Flags = new();
		private List<ResourceLocationData> _locationData = new();

		private AgencyFlagsData _agencyFlagsData;

		public override void OnInitialized()
		{
			_agencyFlagsData = Resources.FindObjectsOfTypeAll<AgencyFlagsData>()[0];
			
			var dirInfo = new DirectoryInfo("flags");
			Logger.LogInfo($"Loading {dirInfo.GetFiles("*.png").Length} new flags.");
			foreach (var fileInfo in dirInfo.GetFiles("*.png"))
			{
				Logger.LogInfo($"Loading flag from {fileInfo.FullName}");
				LoadCustomFlag(fileInfo);
			}
			
			GameManager.Instance.Game.Assets.LoadByLabel("custom_flag",
				sprite => LoadCustomFlag(sprite, false), delegate(IList<Sprite> _)
				{});
		}

		public void LoadCustomFlag(string file)
		{
			LoadCustomFlag(new FileInfo(file));
		}
		
		public void LoadCustomFlag(FileInfo fileInfo)
		{
			var bytes = File.ReadAllBytes(fileInfo.FullName);
			var texture = new Texture2D(512, 320, TextureFormat.RGB24, false);
			texture.LoadImage(bytes);
			var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			sprite.name = $"custom-flags-file-{fileInfo.Name}";
			LoadCustomFlag(sprite);
		}

		public void LoadCustomFlag(Sprite sprite, bool addAddress = true)
		{
			if (Flags.ContainsKey(sprite.name))
			{
				Logger.LogError($"Flag sprites with duplicate names: {sprite.name}. Only one will load.");
				return;
			}

			var newFlag = new AgencyFlag
			{
				FlagName = sprite.name,
				FlagIcon = sprite,
				PrimaryColor = Color.black,
				SecondaryColor = Color.black
			};
			_agencyFlagsData.Flags.Add(newFlag);
			Logger.LogInfo($"Loaded flag {sprite.name}");
			if (!addAddress) return;
			Flags.Add(sprite.name, new FlagResource
			{
				Texture = sprite.texture,
				Sprite = sprite
			});
			_locationData.Add(new ResourceLocationData(new []{sprite.name}, sprite.name, typeof(FlagResourceProvider), typeof(Sprite)));
		}

		public override void OnPostInitialized()
		{
			Addressables.AddResourceLocator(new ResourceLocationMap("custom-flags-location-map", _locationData));
			Addressables.ResourceManager.ResourceProviders.Add(new FlagResourceProvider());
		}
	}

	public class FlagResourceProvider : ResourceProviderBase
	{
		public override void Provide(ProvideHandle provideHandle)
		{
			if (!CustomFlagsMod.Flags.ContainsKey(provideHandle.Location.PrimaryKey))
			{
				provideHandle.Complete<Sprite>(null, false,
					new ProviderException($"Unknown flag {provideHandle.Location.PrimaryKey}"));
				return;
			}

			if (provideHandle.Location.ResourceType == typeof(Texture2D))
			{
				provideHandle.Complete(CustomFlagsMod.Flags[provideHandle.Location.PrimaryKey].Texture, true, null);
			}
			else
			{
				provideHandle.Complete(CustomFlagsMod.Flags[provideHandle.Location.PrimaryKey].Sprite, true, null);
			}
		}
	}
}