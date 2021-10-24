
namespace MobileDevices.iOS.AfcServices
{
    public enum AfcFileMode : ulong
    {
        FopenRdonly = 0x00000001,

        FopenRw = 0x00000002,

        FopenWronly = 0x00000003,

        FopenWr = 0x00000004,

        FopenAppend = 0x00000005,

        /// <summary>
        /// a+  O_RDWR   | O_APPEND | O_CREAT 
        /// </summary>
        FopenRdappend = 0x00000006
    }

}
