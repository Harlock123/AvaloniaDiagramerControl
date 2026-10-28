# AGENTS.md

Notes for anyone, human or AI agent, changing this code. `README.md` is the user guide and full API reference (it
is also packed into the NuGet package); this file covers how the code is put together and what to watch when
changing it.

## What this is

`AvaloniaDiagramer`, a multi-series line chart control for Avalonia 11 on .NET 9, with polynomial regression trend
lines, a grid, knot markers and a flashing highlight. A demo app shows it with sample data.

## Commands

```bash
dotnet build AvaloniaDiagramerControl.sln                         # .NET 9 SDK; there is no global.json
dotnet run --project AvaloniaDiagramerControl.Demo                # the demo window
dotnet pack AvaloniaDiagramerControl.Controls -c Release          # NuGet package (also made on every build)
```

- `GeneratePackageOnBuild` is on, so every build of the control also writes a `.nupkg` (package id
  `AvaloniaDiagramerControl`, version in the `.csproj`).
- There are no tests yet, and no CI workflow.

## Layout

- `AvaloniaDiagramerControl.Controls/AvaloniaDiagramer.cs`: the whole control, a `Control` that draws itself in
  `Render`.
  - Regions: fields, constructor (default palette), properties, public methods, layout, rendering, event handlers.
  - `RenderDataSeries` parses the series and calls `DrawChartBackground`, `DrawAxes`, `DrawDataLines` (with knots)
    and `DrawTrendLine`; `DrawGrid` and `DrawTitle` run alongside.
- `AvaloniaDiagramerControl.Controls/Regressor.cs`: an internal least-squares polynomial fit used for the trend
  lines (`Degree`, `XYAdd`, `RegVal`).
- `AvaloniaDiagramerControl.Demo/`: `MainWindow` with buttons that load the sample data sets.
- `screenshots/`: the pictures used by the README.

## Rules that keep it working

- **Plain properties, not Avalonia properties.** Each setting is a CLR property with a backing field whose setter
  calls `InvalidateVisual()`. They can be set from XAML but not bound or styled. A new property should follow the
  same pattern, and get a row in the README's API tables.
- **Data is strings.** A series is `"Label,v1,v2,..."` and `XAxisLabel` is `"AxisTitle,tick1,tick2,..."`;
  `AddDataSeries` just stores the string; parsing happens while rendering. `AddDataSeries` does not redraw, so callers
  call `InvalidateVisual()` afterwards.
- **Ten series at most.** Colours, visibility and flashing use fixed arrays of 10 (indices 0-9). Raising the limit
  means changing every array and the default palette together.
- **Flashing** runs on a 500 ms `DispatcherTimer`; `FlashDataLine = -1` and `ClearDataSeries()` stop it and restore
  the saved colours.
- **Line endings.** The C# sources use CRLF; keep each file's existing line endings so diffs show only real changes.

## Making a change

- Run the demo and try both sample data sets after any drawing change, then refresh the screenshots if the look
  changed.
- Bump `<Version>` in the control's `.csproj` before publishing a new package.
