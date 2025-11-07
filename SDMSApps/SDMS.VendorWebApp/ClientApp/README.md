# Vendor Web App - Angular 18 PWA

This is a Progressive Web Application (PWA) built with Angular 18 for the Vendor portal.

## Features

- **Angular 18**: Latest Angular version with standalone components
- **PWA Support**: Service Worker enabled for offline functionality
- **Standalone Components**: Modern Angular architecture
- **TypeScript 5.4**: Latest TypeScript version

## Getting Started

### Prerequisites

- Node.js (v18 or higher)
- npm (v9 or higher)

### Installation

1. Navigate to the ClientApp directory:
```bash
cd ClientApp
```

2. Install dependencies:
```bash
npm install
```

### Development

Run the development server:
```bash
npm start
```

The app will be available at `http://localhost:4200/`

### Building for Production

Build the application:
```bash
npm run build
```

The production build will be in the `dist/vendor-web-app` directory.

## Project Structure

```
ClientApp/
├── src/
│   ├── app/
│   │   ├── app.component.ts      # Root component
│   │   ├── app.routes.ts         # Route configuration
│   │   └── app.config.ts         # App configuration
│   ├── assets/                   # Static assets
│   ├── index.html                # Main HTML file
│   ├── main.ts                   # Bootstrap file
│   ├── manifest.webmanifest      # PWA manifest
│   └── styles.css                # Global styles
├── angular.json                  # Angular CLI configuration
├── ngsw-config.json              # Service Worker configuration
├── package.json                  # Dependencies
└── tsconfig.json                 # TypeScript configuration
```

## PWA Features

- Service Worker for offline support
- Web App Manifest for installability
- App icons for different sizes
- Theme color configuration

## Notes

- This is a frontend-only application
- No backend files (Program.cs, Startup.cs) are included
- The .csproj file is minimal and only used for solution integration

