# UI kit - title screen

Flat, corporate look: no glows, no transparency effects. Sources are SVG in `Art/UI/svg/`, exported PNGs are in
`Assets/UI/Sprites/Title/`.
`title_screen_mock.png` is the target look. Text in the mock is a stand-in for live TextMeshPro text.

**No text is baked into any sprite.** Title, subtitle and button labels are TMP so they can change.

## Colour tokens
| token | hex | use |
|---|---|---|
| bg900 | #0B141C | scrim colour, deepest background |
| bg800 | #111E29 | panel fill |
| panel | #14222D | secondary button fill |
| line | #2C3E4C | borders, divider |
| steel | #8EA3B3 | secondary text (subtitle) |
| text | #E8EEF2 | primary text |
| muted | #5F7384 | disabled text, footer |
| accent | #2A74B8 | primary button, accent rule |
| accent_hover | #3A8FDB | primary hover (white text 3.4:1, fine for large labels) |
| accent_press | #1F5E96 | primary pressed |

## Sprite import settings
Texture Type = Sprite (2D and UI), Mesh Type = Full Rect for 9-sliced sprites, Filter = Bilinear, no mipmaps,
Compression = None (or High Quality) so edges stay crisp.

| sprite | size | Sprite Editor border (L,B,R,T) | Image Type |
|---|---|---|---|
| btn_primary_* / btn_secondary_* (4 states each) | 128x128 | 24,24,24,24 | Sliced |
| panel_modal | 256x256 | 40,40,40,40 | Sliced |
| divider_h | 256x2 | none | Simple, stretch width |
| accent_rule | 96x4 | none | Simple |
| icon_close / icon_close_hover | 64x64 | none | Simple |
| logo_mark | 512x512 | none | Simple (shown at 128x128) |
| title_scrim | 1920x1080 | none | Simple, stretch to fill the canvas, Raycast Target off |

Buttons: use Button Transition = Sprite Swap (normal = Image sprite, Highlighted = hover, Pressed, Disabled).

## Title screen layout (1920x1080 reference, origin top-left, text column left-aligned at x=180)
Layer order, back to front: freeze-frame RawImage (full screen, aspect-fill) -> title_scrim (full screen) -> column.

| element | position / size | style |
|---|---|---|
| logo_mark | x180 y140, 128x128 | |
| Title (TMP) | x180, top of text ~y304, 2 lines | Archivo Condensed Medium, 132 px, uppercase, character spacing ~+5%, line height ~102%, #E8EEF2 |
| Subtitle (TMP) | x180, ~22 px below title | Archivo, 30 px, spacing ~+4%, #8EA3B3 |
| accent_rule | x180 y~676, 96x4 | |
| PLAY button | x180 y736, 420x76 | primary sprites; label Archivo Condensed Medium 36 px, spacing ~+12%, white |
| OPTIONS button | x180 y832 (20 px gap), 420x76 | secondary sprites; same label style |
| Footer (TMP) | x180, ~64 px from bottom | Archivo, 20 px, #5F7384 |

Title, subtitle and footer should come from data (not hard-coded) since they are expected to change.
Which freeze-frame to use behind the title screen is still open.

## Options panel
panel_modal (Sliced) centred, about 60% x 60% of the screen. Heading label in Archivo Condensed Medium 34-40 px, divider_h
under it, icon_close top-right (swap to icon_close_hover on hover). Slider and toggle sprites are not made yet.

# UI kit - choice screen

Target looks: `choice_mock_with_description.png` and `choice_mock_no_description.png` (text is a stand-in for live TMP text).
Sprites are in `Assets/UI/Sprites/Choice/`, SVG sources in `Art/UI/svg/`.

## Sprite import settings
Same as the title screen kit (Sprite 2D and UI, Bilinear, no mipmaps, no compression).

| sprite | size | border (L,B,R,T) | Image Type / use |
|---|---|---|---|
| choice_btn_normal / hover / pressed / selected / disabled | 128x128 | 24,24,24,24 | Sliced. Button Transition = Sprite Swap: normal = Image sprite, Highlighted = hover, Pressed = pressed, Selected = selected (keyboard / gamepad focus), Disabled = disabled |
| text_panel | 256x256 | 40,40,40,40 | Sliced, optional dark panel behind title and description if a bright video frame hurts legibility |
| icon_chevron | 48x48 | none | Simple, white so it can be tinted. Show at the right end of a choice only while it is hovered or selected |
| choice_scrim | 1920x1080 | none | Simple, stretched to fill the canvas, Raycast Target off. Transparent over the top 36%, then darkens toward the bottom |

The 6px accent bar on the left edge of each choice button is part of the sprite and stays 6px when the button is stretched.

## Layout (1920x1080 reference, origin top-left)
Layer order, back to front: video / freeze-frame (full screen, aspect-fill) -> choice_scrim (full screen) -> left text block and right choice stack.

| element | position / size | style |
|---|---|---|
| Left text block | x120, width 860, bottom edge 96 px above the bottom of the screen | |
| Scene title (TMP) | top of the left block | Archivo Condensed Medium, 76 px, uppercase, spacing ~+4%, line height ~105%, #E8EEF2 |
| accent_rule | 22 px under the title, x120, 96x4 | |
| Description (TMP) | 26 px under the rule, width 860 | Archivo, 30 px, line height ~150%, #C5D1DA |
| Choice stack | x1060, width 740, bottom edge 96 px above the bottom of the screen | vertical, 3 buttons, minimum 84 px tall, 16 px gap, anchored to the bottom so 1 or 2 choices still sit low; buttons grow upward when a label wraps (see below) |
| Choice label (TMP) | 40 px left padding, 34 px right padding, 14 px top and bottom padding | Archivo Medium (500), 32 px, #E8EEF2, left-aligned, word wrap on, line height ~115% |

The choice stack top lands at about y=700, which keeps it below the main subject in the current freeze-frames.

## Optional description text
Add a setting that hides the description (for example `ShowDescription`, default on).
- When off, the left block holds only the title and accent rule, bottom-aligned with the choices, as in `choice_mock_no_description.png`.
- Nothing else moves: the choice stack is anchored independently of the left block.
- This is useful before the videos exist; it should be a serialized field or a runtime toggle that is easy to flip.

## Two-line choice labels (fallback)
Labels wrap at about 42 characters. See `choice_mock_two_line.png`.
- Each button has a minimum height of 84 px and grows with its label (Layout Element min height 84, Vertical Layout Group with Control Child Size height on, label preferred height driving it). A two-line button is about 102 px; the other buttons stay 84 px.
- The stack is bottom-anchored, so a taller button pushes the ones above it up. Worst case (three two-line choices) the stack top lands near y=646, still just below the guards' feet in the current frames.
- Keep it to two lines. A third line makes the button very tall, so set the label to TMP auto-size, min 26 px, max 32 px, overflow = Ellipsis, so extremely long labels shrink first and then truncate instead of growing forever.
- The chevron stays vertically centred and the 6 px accent bar stretches to the full height automatically (9-slice).
