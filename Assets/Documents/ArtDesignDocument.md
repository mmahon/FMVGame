---
description: Art style and visual design for the FMV game "They Cloned Me" (scene imagery and UI).
applicability: Use whenever generating art assets, writing image or video prompts, or making visual design decisions.
---

# Art Design Document - "They Cloned Me"

## Visual Identity

### Core Aesthetic
**Corporate research-facility thriller**
- A believable, near-future secure research site (the fictional Arkos Research Initiative), seen as photoreal still frames and video
- Cold, controlled and quietly menacing: institutional rather than gothic
- Clean surfaces, hard linear lighting and long sight-lines, with tension from being watched (cameras, glass walls, guards)
- Realistic rendering with deliberate, cinematic composition
- Reference for structure and tone: modern FMV games such as The Spectre Files

### What it is not
- Not film noir: no warm desk lamps, old wood, brass, smoke or heavy chiaroscuro
- Not holographic or "cyberpunk": no neon, glows or floating interface elements, in scenes or in the UI

## Setting and Locations
The story moves inward through one facility. Known locations so far:
- **Perimeter** (exterior, dusk, rain): gate, guard booth, delivery truck, wooded fence line
- **Loading dock** (inside the truck and dock): grey steel panelling, wooden crates, fluorescent strips
- **Vents and ductwork**: tight, dim and grimy: riveted metal, dust, cobwebs, a muted olive-grey cast
- **Basement / sub-basement**: dark industrial lab storage with hissing pipes
- **Admin hallway and reception**: pristine, bright, high security
- **Security closet** (captured scenes): dim and cramped
- **Server room**: referenced in the story, no imagery yet

## Color Palette

### Base
- **Cool steel**: brushed stainless, grey concrete, blue-grey shadows
- **Desaturated**: low saturation overall; colour comes from the light sources and a few accents
- **Dark values** in restricted areas (blue-black), **bright cool whites** in public areas

### Accents (use selectively)
- **Warm amber/yellow**: a lit guard booth window, a window in the dark, wooden crates. Warmth marks people and the organic against the cold facility
- **Red**: danger and alarms (tail lights, fire pipes, indicator lights)
- **Safety yellow**: floor striping, bollards, hazard marks
- **Cyan-blue LED**: shelf and strip lighting in labs, and the same hue family as the UI accent

### Area Palettes
| Area | Look |
|---|---|
| Perimeter (exterior) | Overcast blue-grey sky, green-black forest, wet gravel; one warm point of light (the booth) |
| Back of house (dock, basement) | Grey steel and concrete, blue LED strips, red and yellow utility accents, deep shadows |
| Vents and ducts | Dim olive-grey metal, dust and cobwebs, little colour, very low light |
| Public admin (hallway, reception) | Bright silver and white, frosted glass, cool white linear lighting, very clean |
| Captured / confinement | Dim, close, frosted-glass silhouettes, sparse furniture |
| Endings | Failure endings: cold and desaturated. Success ending: lighter and cleaner. Add specifics here when the endings are produced |

There is no "supernatural" palette in the current story. If a later scene needs an uncanny or reveal look, treat it as a deliberate exception and document it here.

## Lighting Philosophy

- **Motivated**: every light is a plausible fixture (fluorescent strips, LED shelf strips, floodlights, a booth lamp, ceiling panels)
- **Linear sources**: long strip lights and ceiling panels that lead the eye down a corridor
- **Contrast between zones**: bright and sterile in public areas, dark and cluttered behind the scenes
- **Pools, not spotlights**: light falls in pools on benches, floors and key objects; no theatrical spot beams
- **Shadow as threat, not style**: dark areas hide guards, doors and exits

### Lighting Moods
- **Stealth / tension**: low-key, cold, a single bright source in the distance
- **Discovery**: a clean, lit surface with the key object on it (the basement bench with the key card)
- **Exposure**: bright, even, shadowless light where the player could be seen (the hallway)
- **Captured**: dim and flat, with a bright edge of door light

## Composition

### Cinematic Framing
- **Eye-level wide shots** dominate; the player character is generally not shown
- **One-point perspective** corridors and docks are common. Offset the focal subject slightly or use foreground clutter to avoid dead symmetry
- **Depth**: foreground props, midground subject, background door or light
- **Leading lines**: ceiling strips, floor stripes and rails point at the subject

### UI Safe Areas (important)
Freeze-frames are shown full screen with text and buttons over them. Compose for that:
- Keep the **main subject above roughly 65% of the frame height**. The choice buttons sit in the right half from about 65% down, and the title and description sit bottom-left
- Keep the **lower third low in detail** (floor, foreground, soft shadow) so text stays legible
- Keep important content within the **central ~90%**; the image is aspect-filled and may be cropped on non-16:9 screens
- The title screen darkens the left ~40% with a scrim, so keep any key subject right of centre in that frame
- No text, captions or interface burned into the image

### Camera Angles
- **Wide establishing**: each new location
- **Medium and close**: hands, doors, card readers, objects the player can use
- **High angle**: vents and ducts, security cameras
- **Low angle**: guards and imposing doors, used sparingly

## Atmospheric Elements

- **Rain and damp** outside: wet gravel, puddles, mist near the tree line
- **Steam and hiss** in the basement: pipes, vents, floor sheen
- **Sterile gloss** in the admin areas: polished floors, reflections on steel and glass
- **Haze** only when motivated (rain, steam); no decorative fog or dust beams
- **Lens**: natural; slight vignette is fine; no lens flares or heavy motion blur outside action

## Material Qualities
- **Metal**: brushed stainless, galvanized panels, painted steel racks
- **Glass**: frosted and clear partition walls, often showing blurred figures beyond
- **Concrete**: polished or sealed floors, rough walls in back of house
- **Cardboard and timber**: crates and boxes, the one warm, organic texture inside the facility
- **Lab equipment and cables**: labelled storage boxes, instruments, cable coils
- **Surface treatment**: realistic, with restrained reflections. Public and lab areas are clean and well kept; the vents and the exterior are the dirty exceptions (dust, cobwebs, mud, wet gravel). Contrast between clean and grimy helps tell the zones apart

## Scene-Type Visual Guidelines

### Location / Hub Scenes
- Wide establishing shot with the exits or doors that lead to the choices visible
- A readable layout so the player understands the options

### Item / State-Change Scenes
- Reuse the same frame with the object present or absent so the change reads clearly. Example: `a2_basement` shows the key card on the bench; `a2_basement_keycard` and `_gone` show the empty bench
- The object is lit and in the foreground

### Action / Transition Scenes
- Short clips (climbing, hiding, running) that auto-advance. Keep the camera readable and consistent with the surrounding location

### Captured Scenes
- Close and dim, with a clear threat (a guard silhouette through frosted glass, zip-ties, a chair)

### Ending Scenes
- Tailor to the outcome per the Area Palettes table; give each ending a distinct but consistent look

## Signage and Text in Images
- Use the fictional name **Arkos Research Initiative** for any facility signage; avoid real brands
- Generated text is often garbled. Keep legible text minimal and check it. Prefer short labels (`STORAGE 55 - RESTRICTED`, `CHECK-IN HERE`)
- No generator watermarks. Regenerate or crop any frame that has one

## Technical Specifications

### Image Format
- Freeze-frame images are loaded by scene id: `Resources/FMV/<GameName>/Images/<scene_id>.png|jpg|jpeg` (or `Resources/FMV/Images/` by default)
- Matching videos go in `Videos/` next to it as `<scene_id>.mp4`
- Aim for **16:9, about 1920x1080 or larger**
- Displayed full screen as a `RawImage`, aspect-filled (see UI Safe Areas above)
- The freeze-frame appears when the scene loads and again as the backdrop during the choice phase, so it should match the clip's **final frame** as closely as possible to avoid a visible jump, and be clean enough to show before playback starts
- If a scene has no freeze-frame, the UI falls back to the clip's last frame, or a default texture
- Media files are stored with Git LFS (see `.gitattributes`)

## Consistency Guidelines

### Maintaining Cohesion
- Every scene shares the cold, clean, corporate facility look
- Lighting is always motivated; palettes follow the Area Palettes table
- Colour restraint: warm and red accents are rare and meaningful
- Materials stay consistent across locations (steel, glass, concrete, with crates as the warm exception)

### Visual Progression
- Moving inward, spaces get **cleaner and brighter but feel more watched** (cameras, glass walls, reception)
- Back-of-house spaces are the dark, tactile, hiding places; public spaces are the exposed ones
- Endings carry a distinct visual signature

## UI Design

The UI is flat, clean and corporate. It sits over the freeze-frames and must never compete with them.

- **Detailed spec**: `Art/UI/README.md` (tokens, sprite import borders, layouts, text styles). Mocks are in `Art/UI/`
- **Look**: flat fills, 2px borders, soft rounded corners (10 px radius on buttons), one blue accent. No glow, bloom, transparency tricks or hologram effects
- **Type**: Archivo Condensed Medium for titles and button labels, Archivo for body text, both from `Assets/Fonts/`
- **Colour tokens** (full list in the README): deep navy surfaces (`#0B141C`, `#111E29`, `#14222D`), steel line `#2C3E4C`, steel text `#8EA3B3`, primary text `#E8EEF2`, accent blue `#2A74B8` (hover `#3A8FDB`, pressed `#1F5E96`)
- **Scrims** darken the freeze-frame behind text (a left-to-right scrim on the title screen, a bottom scrim on the choice screen)
- **No text baked into sprites**: titles, descriptions and button labels are live TextMeshPro text
- **Sprites are authored as SVG** in `Art/UI/svg/` and exported to PNG in `Assets/UI/Sprites/`
- **Logo mark**: two overlapping circles, a solid blue circle and a white-outlined one with a white overlap (the clone motif)

## Future Expansion

### Additional Scenes
New scenes should:
- Reference an existing scene type and area palette
- Follow the UI safe areas so the choice screen stays readable
- Keep lighting motivated and the facility believable
- Reuse established locations before adding new ones (server room, reception, labs)

### Video Production
- Match the freeze-frame and clip final frame
- Keep the same cool grade and motivated lighting across clips
- Use the stills as the cinematography reference for each location
