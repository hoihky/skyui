---
title: Video timeline roadmap
order: 56
---

`VideoTimeline` is a multi-track sequencing control in `SkyUI.Controls`. It supports scrubbing, zoom, clip move/trim, markers, clipboard editing, and basic playback. This document defines the path from the current prototype to a control suitable for **video editors** and **sprite animation editors**.

## Current state (v1)

| Area | Status |
|------|--------|
| Multi-track lanes, clips, markers | Done |
| Playhead, ruler scrub, Ctrl+wheel zoom | Done |
| Clip move, trim, delete, copy/cut/paste | Done |
| Track add/remove/reorder, track selection | Done |
| Time-range selection (lane drag) | Done |
| MVVM collections (`Tracks`, `Clips`, `Markers`) | Done |
| Automated tests | 76+ unit tests, 13 headless tests (themed + API) |
| Snap, split, undo | Done (Phase 1) |
| Frame ruler, transport, snap-to-frame | Done (Tier 0 / Phase 2 foundation) |
| Sprite clip metadata, track kind, frame API, JSON DTO | Done (Tier 1) |
| Hold frames, draggable markers, onion skin, property keyframes | Done (Tier 3) |
| Media clock, preview sync, compositor demo, thumbnail contract | Done (Tier 4) |
| Thumbnails, waveforms, keyframes | Phase 4+ |

Architecture: `VideoTimeline` is a thin Avalonia host (~350 lines). Editing, rendering, and input are delegated to `TimelineHost` and layered services below.

## Design goals

1. **SOLID** — model, layout math, snap, commands, and gestures are separate types with narrow interfaces.
2. **Extensible** — new snap providers, edit commands, and interactors register without modifying core model types.
3. **Non-breaking** — existing bindable properties and events remain; new APIs are additive.
4. **Testable** — pure logic (snap, split, undo) covered by unit tests; headless tests for public API where practical.

## Target architecture

```text
SkyUI.Controls.Timeline
├── Model/
│   ├── TimelineProject.cs      Tracks, clips, markers, keyframes, duration
│   ├── TimelineTrack.cs        (+ TimelineTrackItem compat alias)
│   ├── TimelineClip.cs         (+ TimelineClipItem compat alias)
│   ├── TimelineMarker.cs       (+ TimelineMarkerItem compat alias)
│   ├── TimelineKeyframe.cs     (Phase 6 sprite lanes)
│   └── TimelineTimeUnit.cs     Seconds / Frames
├── Commands/
│   ├── ITimelineEditCommand.cs (+ ITimelineCommand alias)
│   ├── TimelineUndoStack.cs
│   └── *Command.cs             Move, trim, split
├── Layout/
│   ├── ITimelineLayoutEngine.cs
│   ├── TimelineLayoutEngine.cs Time ↔ pixels + snap grid
│   └── TimelineCoordinateSystem.cs
├── Rendering/
│   ├── TimelineRenderer.cs     Ruler, headers, lanes, clips, overlays
│   └── TimelineLaneVirtualizer.cs  Viewport track windowing
├── Input/
│   ├── ITimelineGestureInteractor.cs
│   ├── TimelineGestureCoordinator.cs
│   ├── TimelineInteractionContext.cs
│   └── *GestureInteractor.cs   Zoom, keyboard, scrub, lane, clip, header
├── Editing/
│   └── TimelineClipOperations.cs
├── Selection/
│   └── TimelineSelectionModel.cs
├── TimelineHost.cs             Wires model ↔ layout ↔ render ↔ input
└── VideoTimeline.cs            Thin templated control + public API
```

**Extension points (Phase 1+):**

- `ITimelineSnapTargetProvider` — add custom snap lines (markers, beats, keyframes).
- `ITimelineEditCommand` / `ITimelineCommand` — plug new undoable edits (ripple delete, slip edit).
- `ITimelineGestureInteractor` — register custom tools via `VideoTimeline.RegisterGestureInteractor`.
- `TimelineClipItem.Tag` / `TimelineTrackItem.Tag` — host app media metadata until typed clip models land in Phase 3.

## Phased delivery

### Phase 1 — Stability and editor fundamentals (done)

| Item | Description |
|------|-------------|
| Unit tests | Snap, split, undo stack, coordinate math |
| `TimelineSnapEngine` | Snap to playhead, markers, clip edges, grid |
| `TimelineUndoStack` | Command pattern undo/redo |
| Split at playhead | `SplitClipAtPlayhead()` |
| Multi-clip move | Drag moves all selected clips |
| Marquee select | Ctrl+drag on lane background selects clips |
| Selection model | Centralized clip/track selection + events |
| Edit events | `ClipChanging` / `ClipChanged` |
| Theme tokens | Replace hardcoded lane separator colors |
| Scroll stability | Preserve offsets across rebuild; incremental lane virtualization on vertical scroll |
| Ruler drag scrub | Press + drag on ruler updates playhead |
| Themed headless tests | Clipboard paste and template wiring verified with Sky theme |

**Exit criteria:** Demo shows snap toggle, split, undo/redo; 70+ unit tests and themed headless coverage; no regressions in other SkyUI controls. **Met.**

### Phase 2 — Time modes and transport (foundation done)

- FPS and frame-based ruler; frame labels (`fN`) on ruler when `TimeUnit` is `Frames`
- `VideoTimeline.Fps`, `TimeUnit`, `PlayheadFrame`, `LoopTimeRange`
- Frame step (←/→, Shift = 10), Space play/pause; loop in/out from time-range selection
- Snap-to-frame grid when `TimeUnit` is `Frames` (`TimelineTimePresentation` + `TimelineFrameQuantizer`)
- Remaining: timecode ruler option in UI, J/K/L shuttle

### Tier 1 — Sprite clip model (done)

- `TimelineSpriteClipMetadata` on clips (`AtlasId`, `SpriteName`, `FrameIndex`, `HoldFrames`, optional `SourceRect`)
- `TimelineTrackKind` + `IsVisible` / `IsLocked` / `AccentColor` on tracks; locked lanes block move/trim
- `ITimelineClipFrameMapper`, `GetClipFrameSpan`, `SampleSpriteLayersAtPlayhead`
- `CurrentFrameChanged` for preview sync
- `TimelineProjectDocument` + `ITimelineProjectSerializer` (`JsonTimelineProjectSerializer`)

### Phase 3 — Rich clip and track model

- `SourceIn` / `SourceOut`, speed, linked clips
- Mute/solo, variable height (visibility/lock cover sprite workflows)
- Draggable markers

### Phase 4 — Visual richness

- Clip thumbnails and audio waveforms (async, cached)
- Sprite sheet region preview on clips
- Minimap and zoom-to-selection

### Phase 5 — Pro editing tools

- Ripple delete, roll/slip edits, transitions overlap
- Linked A/V selection

### Phase 6 — Sprite animation

- Cel duration in frames, onion skin, per-layer visibility keys
- Transform keyframe lanes

### Tier 4 — App integration (done)

- `ITimelineMediaClock` / `TimelineMediaClock` on `VideoTimeline.MediaClock`
- `TimelinePreviewSynchronizer`, `TimelinePreviewFrameSnapshot`, `PreviewFrameChanged`
- `ITimelineClipThumbnailProvider` + `NullTimelineClipThumbnailProvider`
- Demo: compositor preview panel bound to preview snapshots (atlas color stub)

### Phase 7 — Platform integration

- Touch gestures, full project persistence in host apps

## Video editor vs sprite editor

| Need | Video path | Sprite path |
|------|------------|-------------|
| Time base | Timecode, source trim | Frames + FPS |
| Clip data | `SourceIn`/`SourceOut`, linked audio | Frame index, atlas region |
| Lane visuals | Thumbnails, waveforms | Cel preview, onion skin |
| Editing | Ripple, split, slip | Keyframes, hold frames |

Both share Phase 1 (snap, undo, split, stable selection) and the command/undo pipeline.

## Demo and docs

- `SkyUI.Demo` → **Video timeline** page exercises the control.
- After each phase, update this roadmap and `current-state.md`.
- Regenerate HTML: `./docs/build-docs.sh` (requires sibling MDWeb repo).

## Related

- [Development roadmap](development-roadmap.md) — product phases
- [Current state](current-state.md) — shipped controls inventory
