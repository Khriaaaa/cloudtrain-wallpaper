# 云间列车 · WebGL 着色器壁纸

实时渲染的云海列车场景，纯 WebGL 单文件实现。基于 Shadertoy 作品
[Traveling in the Cloud](https://www.shadertoy.com/view/Ndc3zl)（作者 Tianxiu Zhou）改造，
加入列车变速、昼夜循环、天气系统和自定义素材。

## 当前版本

**beta1** — 2026-09-20

## 功能

- **昼夜循环**：日出日落、黄昏暖调、月升月落带环形山
- **天气**：雨（含雨花像素水花）、雪（渐进积雪）、雷暴、风
- **星空**：程序化生成，无 sin 哈希，约 100 颗带光晕的星点
- **列车**：过桥变速，夜间车头扇形光柱
- **其他**：飞机尾迹、飞鸟、云海多层视差

## 参数面板

页面右下角「参数」按钮可实时调整缩放、偏移、速度、振幅、细节、雨量、
雷暴、积雪、风力，以及曝光、饱和度、色相、色温、暗角等后处理参数。

URL 参数：`?t=<秒>&tod=<0-1 时刻>&ui=0`（隐藏 UI，便于截图）

## 文件说明

| 文件 | 说明 |
|---|---|
| `wallpaper/index.html` | 主程序，单文件自包含 |
| `wallpaper/blue_noise.png` | 噪声纹理，供着色器采样 |
| `src/` | Windows 壁纸宿主（C# / WebView2） |
| `installer/` | NSIS 安装脚本 |
| `CloudTrainWallpaper-Setup.exe` | 已编译安装包 |

## 部署

直接当网页跑：

```bash
python3 -m http.server 8099 --bind 0.0.0.0 --directory wallpaper
```

或双击 `CloudTrainWallpaper-Setup.exe` 装成 Windows 动态壁纸。

## 授权

原着色器版权归原作者 Tianxiu Zhou。改造部分由 Kyria 完成，仅供个人使用。
