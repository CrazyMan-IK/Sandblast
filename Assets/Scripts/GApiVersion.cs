using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

namespace Sandblast
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class GApiVersion : MonoBehaviour
    {
        private void Awake()
        {
            var builder = new StringBuilder();
            builder.Append(SystemInfo.graphicsDeviceID);
            builder.AppendLine();
            builder.Append(SystemInfo.graphicsDeviceName);
            builder.AppendLine();
            builder.Append(SystemInfo.graphicsDeviceType);
            builder.AppendLine();
            builder.Append(SystemInfo.graphicsDeviceVendor);
            builder.AppendLine();
            builder.Append(SystemInfo.graphicsDeviceVendorID);
            builder.AppendLine();
            builder.Append(SystemInfo.graphicsDeviceVersion);
            builder.AppendLine();
            builder.Append(SystemInfo.graphicsMemorySize);
            builder.AppendLine();
            builder.Append(SystemInfo.graphicsMultiThreaded);
            builder.AppendLine();
            builder.Append(SystemInfo.graphicsShaderLevel);
            builder.AppendLine();
            builder.Append(SystemInfo.graphicsUVStartsAtTop);
            builder.AppendLine();
            builder.Append(SystemInfo.supportsGraphicsFence);

            GetComponent<TextMeshProUGUI>().text = builder.ToString();
        }
    }
}
