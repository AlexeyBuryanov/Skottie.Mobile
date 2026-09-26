# Contributing to Skottie.Mobile

Bug reports, documentation improvements, fixes, and focused feature contributions
are welcome. Please follow the [Code of Conduct](CODE_OF_CONDUCT.md) in all project
interactions.

## Before you start

Search the [existing issues](https://github.com/AlexeyBuryanov/Skottie.Mobile/issues)
and pull requests to avoid duplicating work. Open an issue to discuss substantial
API changes, new platforms, or new dependencies before implementing them. Small
fixes and documentation improvements can go directly into a pull request.

For a bug report, include:

- Expected behavior, actual behavior, and steps to reproduce it.
- The affected control: native Android, native iOS, or MAUI.
- .NET SDK and workload versions (`dotnet --info` and `dotnet workload list`).
- OS version, device or simulator model, and relevant logs or exceptions.
- A minimal reproduction, ideally using one of the playground apps.
- A small Lottie JSON example when the problem is animation-specific, provided
  you have permission to share it.

Remove credentials, personal data, and proprietary assets before uploading a
reproduction or logs.

## Development setup

1. Fork the repository and clone your fork.
2. Create a branch from `main` for your change.
3. Install the .NET 10 SDK and the workloads required for your target platform.
4. Open `Skottie.Mobile.slnx` in your IDE, or use the commands below.

For development across all supported platforms:

```sh
dotnet workload install android ios maui
dotnet restore Skottie.Mobile.slnx
dotnet build Skottie.Mobile.slnx
```

Android development requires a compatible Android SDK and JDK. Building and
running iOS applications requires a Mac with compatible Xcode, or a paired Mac
when using Visual Studio on Windows. See the [README requirements](README.md#requirements-and-building).

If you are working only on Android, build the relevant project without requiring
an iOS application build:

```sh
dotnet build src/Skottie.Mobile.Android/Skottie.Mobile.Android.csproj
dotnet build src/Skottie.Mobile.Maui/Skottie.Mobile.Maui.csproj -p:TargetFrameworks=net10.0-android
```

## Repository layout

| Path | Purpose |
| --- | --- |
| `src/Skottie.Mobile.Android` | Android control and frame scheduling |
| `src/Skottie.Mobile.iOS` | iOS control and frame scheduling |
| `src/Skottie.Mobile.Maui` | Bindable control, handler, and registration |
| `playground/` | Native and MAUI apps for exercising the controls |
| `playground/Shared/orbit.json` | Shared sample animation |
| `Directory.Packages.props` | Central NuGet dependency versions |

## Making changes

- Follow the existing C# formatting, naming, and nullable-reference conventions.
- Keep pull requests focused; avoid unrelated reformatting or refactoring.
- Keep native rendering and playback in the platform libraries. The MAUI handler
  should reuse those controls rather than duplicate their rendering logic.
- Treat view operations as UI-thread operations. Stop frame callbacks and release
  resources when a view is detached, cleared, or disposed, as appropriate.
- Preserve existing API behavior unless a breaking change has been discussed.
- Add or update dependency versions in `Directory.Packages.props`; do not put
  versions directly on `PackageReference` items.
- Update usage documentation and playground examples when public behavior changes.
- Do not commit build output, packages, signing identities, provisioning profiles,
  credentials, or local IDE settings.

## Validating changes

Build the affected projects. For rendering, playback, or lifecycle changes, run
the relevant native playground and the MAUI playground on the affected platform.
Changes to the shared MAUI API or handler should be checked on both Android and
iOS when possible.

Use the [playground run instructions](README.md#playground-apps) and
[manual checks](README.md#manual-checks). Check loading from a packaged resource
and a file, pause/resume, reload, clear, a missing resource, and background/foreground
transitions. For rendering fixes, also check the animation that reproduced the bug.

A successful build does not verify device rendering. In the pull request, state
which commands you ran and which devices or simulators you tested. If you cannot
test a platform, say so explicitly. Add a focused regression test when the change
can be tested meaningfully; documentation-only changes do not require a mobile build.

## Submitting a pull request

Target `main` and include:

- The problem being solved and a linked issue, if one exists.
- What users will observe after the change.
- Relevant validation results and any platforms that remain untested.
- Screenshots or a short recording for visible rendering or layout changes.
- Any compatibility impact or limitations reviewers should consider.

Keep the description current if the implementation changes during review. Respond
to feedback constructively and explain tradeoffs when there is more than one
reasonable approach.

## Licensing

Contributions are accepted under the project's [MIT license](LICENSE). Submit only
code and assets that you have the right to contribute, and preserve applicable
copyright and attribution notices.
