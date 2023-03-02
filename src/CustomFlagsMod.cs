using System.Collections.Generic;
using System.IO;
using BepInEx;
using KSP.Game;
using KSP.Modding;
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
	
	[BepInPlugin("us.adamsogm.CustomFlags", "CustomFlags", "2.0.0")]
	[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
	public class CustomFlagsMod : BaseSpaceWarpPlugin
	{
		public static readonly Dictionary<string, FlagResource> Flags = new Dictionary<string, FlagResource>();
		
		private AgencyFlagsData _agencyFlagsData; 
		public override void OnInitialized()
		{
			var foundAssets = Resources.FindObjectsOfTypeAll<AgencyFlagsData>();
			_agencyFlagsData = foundAssets[0];
			var dirInfo = new DirectoryInfo("flags");
			Logger.LogInfo($"Loading {dirInfo.GetFiles("*.png").Length} new flags.");
			var locationData = new List<ResourceLocationData>();
			foreach (var file in dirInfo.GetFiles("*.png"))
			{
				var bytes = File.ReadAllBytes(file.FullName);
				var texture = new Texture2D(512, 320, TextureFormat.RGB24, false);
				texture.LoadImage(bytes);
				var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
					new Vector2(0.5f, 0.5f));
				sprite.name = $"custom-flags-file-{file.Name}";
				var newFlag = new AgencyFlag
				{
					FlagName = file.Name,
					FlagIcon = sprite,
					PrimaryColor = Color.black,
					SecondaryColor = Color.black
				};
				Logger.LogInfo($"Loaded new flag {file.Name}");
				_agencyFlagsData.Flags.Add(newFlag);
				Flags.Add(sprite.name, new FlagResource
				{
					Texture = texture,
					Sprite = sprite
				});
				locationData.Add(new ResourceLocationData(new []{sprite.name}, sprite.name, typeof(FlagResourceProvider), typeof(Sprite)));
			}
			Addressables.AddResourceLocator(new ResourceLocationMap("custom-flags-location-map", locationData));
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