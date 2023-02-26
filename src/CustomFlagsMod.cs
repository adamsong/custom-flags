using System;
using System.Collections.Generic;
using System.IO;
using I2.Loc;
using KSP.Game;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Mods;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CustomFlags
{

	public class FlagResource
	{
		public Texture2D Texture;
		public Sprite Sprite;
	}
	
	[MainMod]
	public class CustomFlagsMod : Mod
	{
		public static readonly Dictionary<string, FlagResource> Flags = new Dictionary<string, FlagResource>();
		public static BaseModLogger FlagLogger;
		
		private AgencyFlagsData _agencyFlagsData; 
		public override void Initialize()
		{
			FlagLogger = Logger;
			var foundAssets = Resources.FindObjectsOfTypeAll<AgencyFlagsData>();
			_agencyFlagsData = foundAssets[0];
			var dirInfo = new DirectoryInfo("flags");
			_agencyFlagsData.Flags.ForEach(flag => Logger.Info(flag.FlagIcon.name));
			Logger.Info($"Loading {dirInfo.GetFiles("*.png").Length} new flags.");
			var locationData = new List<ResourceLocationData>();
			foreach (var file in dirInfo.GetFiles("*.png"))
			{
				var bytes = System.IO.File.ReadAllBytes(file.FullName);
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
				Logger.Info(newFlag.FlagIcon.name);
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
			CustomFlagsMod.FlagLogger.Info($"PROVIDING RESOURCE ID {provideHandle.Location.PrimaryKey} {provideHandle.Location.ResourceType.ToString()}");
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