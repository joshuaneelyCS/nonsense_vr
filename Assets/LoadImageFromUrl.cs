using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadImageFromUrl : MonoBehaviour
{
    // public RawImage rawImage;
    // string imageUrl = "https://files.wordscenes.com/title-images/11eacc75741a9336bbcdc306840ae753-vT1SZthRoPkY6trSq.webp";

    // Import WebP decoding functions from the DLL
    [DllImport("libwebp", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr WebPDecodeRGBA(byte[] data, int dataSize, out int width, out int height);

    [DllImport("libwebp", CallingConvention = CallingConvention.Cdecl)]
    private static extern void WebPFree(IntPtr ptr);



    public IEnumerator LoadImage(string imageUrl, RawImage rawImage, Action onComplete)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(imageUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                // Debug.LogError($"Error: This Image does not exist or is of unknown format");
            }
            else
            {
                if (isNotWebp(imageUrl))
                {

                }
                else
                {
                    byte[] webpData = webRequest.downloadHandler.data;
                    Texture2D texture = ConvertWebPToTexture2D(webpData);
                    if (texture != null)
                    {
                        rawImage.texture = texture;
                        rawImage.SetNativeSize(); // Optional: Adjust size to fit the content
                    }
                    else
                    {
                        Debug.LogError("Failed to convert WebP data to Texture2D.");
                    }

                    onComplete?.Invoke();

                }
            }
        }
    }

    private bool isNotWebp(string imageUrl)
    {
        if (imageUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }

    private Texture2D ConvertWebPToTexture2D(byte[] webpData)
    {
        int width, height;
        IntPtr ptr = WebPDecodeRGBA(webpData, webpData.Length, out width, out height);

        if (ptr == IntPtr.Zero)
        {
            Debug.LogError("Failed to decode WebP image.");
            return null;
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(PointerToByteArray(ptr, width * height * 4));
        texture.Apply();
        WebPFree(ptr);

        return texture;
    }

    private byte[] PointerToByteArray(IntPtr ptr, int size)
    {
        byte[] array = new byte[size];
        Marshal.Copy(ptr, array, 0, size);
        return array;
    }
    private void setRectSettings(RectTransform rect, Vector3 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector3 scale)
    {
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.localScale = scale;

    }
}
