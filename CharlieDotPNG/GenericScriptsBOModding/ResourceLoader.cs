using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CharlieDotPNG
{
    public static class ResourceLoader
    {
        public static Texture2D LoadTexture(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var spriteTexture = new Texture2D(0, 0, TextureFormat.ARGB32, false)
            {
                anisoLevel = 1,
                filterMode = 0
            };

            try
            {
                var resourceName = assembly.GetManifestResourceNames().First(r => r.Contains(name));
                var resource = assembly.GetManifestResourceStream(resourceName);
                using var memoryStream = new MemoryStream();
                var buffer = new byte[16384];
                int count;
                while ((count = resource!.Read(buffer, 0, buffer.Length)) > 0)
                    memoryStream.Write(buffer, 0, count);
                spriteTexture.LoadImage(memoryStream.ToArray());
            }
            catch (Exception e)
            {
                Debug.LogError("Missing Texture \"" + name + "\"");
                return LoadTexture("Ally_icon.png");
            }

            return spriteTexture;
        }

        public static Sprite LoadSprite(string name, int ppu = 32, Vector2? pivot = null)
        {
            if (pivot == null) { pivot = new Vector2(0.5f, 0.5f); }
            var assembly = Assembly.GetExecutingAssembly();

            Sprite sprite;

            try
            {
                var resourceName = assembly.GetManifestResourceNames().First(r => r.Contains(name));
                var resource = assembly.GetManifestResourceStream(resourceName);
                using var memoryStream = new MemoryStream();
                var buffer = new byte[16384];
                int count;
                while ((count = resource!.Read(buffer, 0, buffer.Length)) > 0)
                    memoryStream.Write(buffer, 0, count);
                var spriteTexture = new Texture2D(0, 0, TextureFormat.ARGB32, false)
                {
                    anisoLevel = 1,
                    filterMode = 0
                };

                spriteTexture.LoadImage(memoryStream.ToArray());
                sprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), (Vector2)pivot, ppu);

            }
            catch (InvalidOperationException)
            {
                Debug.LogError("Missing Texture \"" + name + "\"! Check for typos when using ResourceLoader.LoadSprite() and that all of your textures have their build action as Embedded Resource.");
                return LoadSprite("DontDoThat_icon.png");
            }

            return sprite;
        }

        public static byte[] ResourceBinary(string name)
        {
            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                var resourceName = a.GetManifestResourceNames().First(r => r.Contains(name));
                using (Stream resFilestream = a.GetManifestResourceStream(resourceName))
                {
                    if (resFilestream == null) return null;
                    byte[] ba = new byte[resFilestream.Length];
                    resFilestream.Read(ba, 0, ba.Length);
                    return ba;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Missing Resource \"" + name + "\"");
                throw e;
            }
        }

    }
}
