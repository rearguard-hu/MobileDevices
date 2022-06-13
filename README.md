# MobileDevices
使用本机协议与 iOS 设备上的服务进行通信的库。
| Build | NuGet |
|--|--|
![](https://github.com/rearguard-hu/MobileDevices/actions/workflows/dotnet.yml/badge.svg)|[![](https://img.shields.io/nuget/v/MobileDevices.svg)](https://www.nuget.org/packages/MobileDevices)|
## Features
MobileDevices 是一个 .NET 库，它允许您使用 .NET 语言（例如 C# 或 Visual Basic）与 Windows、macOS 和 Linux 上的 iOS 设备进行交互。它借鉴于 libimobiledevice 库。

有许多基于设备服务协议实现的客户端:
* 访问设备的文件系统
* 检索有关设备的信息并修改各种设置
* 安装、删除、列出和基本管理应用程序
* 使用官方服务器激活设备
* 检索崩溃报告
* 检索各种诊断信息
* 转发设备通知
## Installing
你可以将 MobileDevices 安装为[NuGet package](https://www.nuget.org/packages/MobileDevices)

```
PM> Install-Package MobileDevices 
```
## Getting Started
MobileDevices 不直接与您的 iOS 设备通信，而是通过**Apple Mobile Device Service**服务进程作为中间件。在连接到 iOS 设备之前，您必须先启动**Apple Mobile Device Service**服务。
如果您想要在**Linux**中运行使用**MobileDevices**构建的程序请先在其中运行一个套接字守护进程，用于在iOS设备之间多路传输连接。关于如何构建套接字守护程序请参考 [usbmuxd](https://github.com/libimobiledevice/usbmuxd) 进行构建。
当然在Windows下您亦可不使用**Apple Mobile Device Service**服务，而参考于 [usbmuxd](https://github.com/libimobiledevice/usbmuxd) 构建Windows下与iOS 设备通信通信的套接字守护进程。进而使用**MobileDevices**操作设备。
### Using the library
在使用这个库前你需要
```c#
using MobileDevices.iOS.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
 {
        services.AddAppleServices();
 }

```
### Listing all iOS devices
以下代码段列出了当前连接到您 PC 的所有iOS设备：
```c#
public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var devices = await _muxerClient.ListDevicesAsync(cancellationToken).ConfigureAwait(false);
            foreach (var device in devices)
            {
                var pairingRecord = await _pairingRecordProvisioner
                    .ProvisionPairingRecordAsync(device.Udid, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
```
### Start device listen
开启设备的监听，这些 DeviceAttachedAsync、DeviceDetachedAsync、DevicePairedAsync 为监听到设备的处理方法，具体细节与实现可根据实际构建 。
以下代码开启了设备监听：
```c#
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
             //Listening for new device events.
            var result = await _muxerClient.ListenAsync(
                DeviceAttachedAsync,
                DeviceDetachedAsync,
                DevicePairedAsync,
                stoppingToken).ConfigureAwait(false);
             //Stopped listening. The client/server closed the connection.
        }
        
```
## Contributing
期待您完善这个库，并感谢每个拉取请求！
如果您有想法修改完善，请在` master` 上创建一个分支，更改、提交并发送拉取请求以供审核。之后它就可以合并到主代码库中。

请确保您的提交符合：
* 提交消息应该很好地描述更改而不是跟简短的概括
* 使用您有效的电子邮件地址进行提交

非常欢迎您的指导与参与！
## Links
* Repository (Mirror): https://github.com/hu766514308/MobileDevices.git
* Issue Tracker: https://https://github.com/hu766514308/MobileDevices/issues
## Credits
Apple、iPhone、iPad、iPod、iPod Touch、Apple TV、Apple Watch、Mac、iOS、iPadOS、tvOS、watchOS 和 macOS 是 Apple Inc. 的商标。

本项目为独立软件，未经 Apple Inc. 授权、赞助或以其他方式批准。
