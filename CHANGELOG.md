<!-- markdownlint-configure-file { "MD024": { "siblings_only": true } } -->
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Radarr Custom Format Support.

## [1.3.3] - 2021-05-06

### Fixed

- Sonarr Quality Definition Max, when set to its maximum value of 400, is now properly set to
  "Unlimited". This is equivalent to the user maxing out the visual slider in the Sonarr UI. Without
  this, some larger sized releases were unintentionally rejected.

## [1.3.2] - 2021-05-05

### Fixed

- Fix exception that occurred when running the create-config subcommand.

## [1.3.1] - 2021-05-05

### Changed

- Executable is now compiled using [Ready to Run]. This substantially increases the size of the
  executable but makes the code much faster.

### Fixed

- Radarr Quality Definition Max and Preferred, when set to their maximum values, are now properly
  set to "Unlimited". Without this, larger sized releases were unintentionally rejected.

[Ready to Run]: https://docs.microsoft.com/en-us/dotnet/core/deploying/ready-to-run

## [1.3.0] - 2021-04-23

### Added

- New configuration for Sonarr release profiles that allows all optional terms to be synced. Look
  for `filter` in the [Configuration Reference] for more details.

[Configuration Reference]: https://github.com/rcdailey/trash-updater/wiki/Configuration-Reference

## [1.2.0] - 2021-04-19

### Added

- New `create-config` subcommand to create a starter YAML config file

## [1.1.0] - 2021-04-18

### Added

- Optional terms in the release profile guides are no longer synchronized to Sonarr.

### Changed

- A warning is now logged when we find a number in brackets (such as `[100]`) without the word
  `score` before it. This represents a potential score and bug in the guide itself.
- Release profile guide parser now skips certain lines to avoid false positives:
  - Skip lines with leading whitespace (i.e. indented lines).
  - Skip admonition lines (lines starting with `!!!` or `???`).

## [1.0.0] - 2021-04-14

See the [Python Migration Guide][py-mig] for details on how to update your YAML configuration.

[py-mig]: https://github.com/rcdailey/trash-updater/wiki/Python-Migration-Guide

### Added

- Full rewrite of the application in C# .NET Core 5
- More than one configuration (YAML) file can be specified using the `--config` option.
- Multiple Sonarr and Radarr instances can be specified in a single YAML config.

### Removed

- Nearly all command line options removed in favor of YAML equivalents.
- Completely removed old python project & source code

[Unreleased]: https://github.com/rcdailey/trash-updater/compare/v1.3.3...HEAD
[1.3.3]: https://github.com/rcdailey/trash-updater/compare/v1.3.2...v1.3.3
[1.3.2]: https://github.com/rcdailey/trash-updater/compare/v1.3.1...v1.3.2
[1.3.1]: https://github.com/rcdailey/trash-updater/compare/v1.3.0...v1.3.1
[1.3.0]: https://github.com/rcdailey/trash-updater/compare/v1.2.0...v1.3.0
[1.2.0]: https://github.com/rcdailey/trash-updater/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/rcdailey/trash-updater/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/rcdailey/trash-updater/compare/v0.1.0...v1.0.0
[0.1.0]: https://github.com/rcdailey/trash-updater/releases/tag/v0.1.0
