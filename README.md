# Velom 🚴

A .NET MAUI application for controlling smart bike trainers with custom workout programming and performance tracking.

![Velom Icon](appicon.svg)

## 🎯 Overview

Velom is a cross-platform mobile application designed for cyclists who want full control over their smart trainer workouts. Whether you're doing structured intervals, custom power targets, or manual resistance control, Velom provides the tools you need for effective indoor training.

## ✨ Key Features

- **Manual Power Control**: Direct control of your smart trainer's resistance
- **Custom Workout Programming**: Create personalized training sessions with:
  - Multiple workout blocks (intervals)
  - Power targets in Watts or % FTP
  - Cadence targets (optional)
  - Ramp intervals with progressive power changes
- **Workout History**: Track and review your completed training sessions
- **Export Capabilities**: Export your workout history in standard format
- **Bluetooth Connectivity**: 
  - Smart trainers (FTMS protocol)
  - Heart rate monitors
  - Tested on Wahoo Kickr Core

## 🔧 Technical Stack

- **.NET 9** with C# 13.0
- **.NET MAUI** for cross-platform UI
- **FTMS Bluetooth Protocol** for trainer communication
- **Local-first**: All data stored locally, no external server communication
- **CommunityToolkit.Mvvm** for MVVM architecture

## 📱 Platform Support

- ✅ **Android**: Currently supported (minimum version TBD)
- 🔜 **iOS**: Coming soon

## 🚀 Getting Started

### Prerequisites

- Visual Studio 2022 or later with .NET MAUI workload
- .NET 9 SDK
- Android SDK (for Android development)
- A FTMS-compatible smart trainer

### Installation

1. Clone the repository:
```bash
git clone https://github.com/Marre-t-eau/Velom.git
cd Velom
```

2. Open `Velom.sln` in Visual Studio

3. Restore NuGet packages

4. Build and deploy to your Android device

### Quick Start Guide

1. **First Launch**: Grant Bluetooth permissions when prompted
2. **Connect Trainer**: Navigate to Control page and connect your smart trainer
3. **Create Workout**: 
   - Go to Workouts List
   - Tap "New Workout"
   - Add blocks with duration, power targets, and cadence
   - Choose between % FTP or Watts mode
   - Save your workout
4. **Start Training**: Select a workout and tap "Start" to begin

## 📂 Project Structure

```
Velom/
├── Sources/
│   ├── Pages/              # MAUI Pages (UI)
│   ├── Objects/            # Data models
│   │   ├── Workout/        # Workout and WorkBlock classes
│   │   └── WorkoutHistory/ # Session recording models
│   ├── Services/           # Business logic services
│   ├── Converters/         # XAML value converters
│   └── Messages/           # MVVM messaging
├── Platforms/
│   └── Android/
│       └── Sources/        # Android-specific Bluetooth implementation
└── Resources/              # Images, styles, fonts
```

### Key Components

- **WorkoutEditorPage**: Create and edit custom workouts
- **WorkoutPage**: Execute workouts with real-time control
- **BaseBikeControlPage**: Core Bluetooth communication logic
- **WorkoutStorageService**: Persistent workout storage
- **AndroidBluetoothManager**: FTMS protocol implementation

## 🎨 Workout Blocks

A workout consists of multiple blocks, each defining:
- **Duration**: Length in seconds
- **Target Power Start**: Initial power (Watts or % FTP)
- **Target Power End**: Final power (for ramps)
- **Target Cadence**: Optional RPM target (leave empty for no target)
- **Power Type**: Watts or % FTP

### FTP Conversion

When using % FTP mode, the app automatically converts percentages to absolute watts based on your configured FTP (Functional Threshold Power).

## 🗺️ Roadmap

- [ ] Graphical workout visualization
- [ ] Web browser support
- [ ] iOS version
- [ ] Pre-built workout library
- [ ] Advanced performance analytics
- [ ] Automated APK distribution

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

### How to Contribute

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 🐛 Bug Reports & Feature Requests

Please create an issue on [GitHub Issues](https://github.com/Marre-t-eau/Velom/issues) if you encounter any problems or have feature suggestions.

## 📄 License

This project is licensed under a **custom license** allowing:
- ✅ Use of source code for personal projects
- ✅ Learning and educational purposes
- ✅ Modifications for personal use
- ❌ Creating a commercial application using only this source code
- ❌ Redistribution as a standalone application without significant modifications

For commercial use or redistribution, please contact the project maintainers.

## 🌐 Links

- **Website**: [cyclingathome.app](https://cyclingathome.app)
- **Repository**: [github.com/Marre-t-eau/Velom](https://github.com/Marre-t-eau/Velom)
- **Branch**: `dev` (active development)

## 🙏 Acknowledgments

Built with:
- [.NET MAUI](https://dotnet.microsoft.com/apps/maui)
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- FTMS Bluetooth specification

---

**Status**: ✅ Functional (Production-ready, manual installation)

Made with ❤️ for the cycling community