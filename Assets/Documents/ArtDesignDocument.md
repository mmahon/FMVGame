---
description: Art style and visual design for the FMV mystery game.
applicability: Use whenever generating art assets or making visual design decisions.
---

# Art Design Document - FMV Mystery Game

## Visual Identity

### Core Aesthetic
**Film Noir Mystery Thriller**
- Inspired by classic detective films and modern FMV games like The Spectre Files
- Moody, atmospheric, and cinematic
- Emphasis on lighting and shadow to create tension
- Realistic rendering with stylized composition

## Color Palette

### Primary Tones
- **Desaturated Base**: Low saturation across most of the image
- **Dark Values**: Heavy use of blacks, deep grays, and dark blues
- **Selective Color**: Strategic use of color to draw attention
  - Warm ambers for intimate/discovery moments
  - Cold blues for tense/mysterious scenes
  - Reds for danger/violence
  - Purples for supernatural/surreal elements
  - Golds for enlightenment/transcendence

### Scene-Specific Palettes
- **Starting Areas**: Cool blues and grays, oppressive darkness
- **Investigation Scenes**: Warm desk lamp ambers, institutional greens
- **Confrontation**: Purple and blue undertones, backlit silhouettes
- **Supernatural**: Vibrant purples and magentas breaking through darkness
- **Endings**: Tailored to emotional tone (gold triumph, red violence, grey ambiguity)

## Lighting Philosophy

### Dramatic Lighting
- **Single Source**: Often one dominant light source creating strong shadows
- **Rim Lighting**: Backlit subjects with edge highlights for silhouettes
- **Motivated Lighting**: Light sources feel natural (desk lamps, windows, streetlights)
- **Shadow as Character**: Darkness is as important as light

### Lighting Moods
- **Mystery**: Side lighting, half-faces in shadow
- **Discovery**: Focused spotlight effect on important objects
- **Tension**: Harsh backlighting, long shadows
- **Revelation**: Bright ethereal light breaking through darkness

## Composition

### Cinematic Framing
- **Rule of Thirds**: Key elements positioned on thirds
- **Leading Lines**: Architecture and objects guide the eye
- **Depth**: Foreground, midground, background layering
- **Symmetry for Impact**: Used sparingly for powerful moments

### Camera Angles
- **Wide Establishing**: For location introduction (hub, library, archives)
- **Medium Shots**: For interaction and dialogue
- **Close-ups**: For evidence examination and emotional beats
- **Low Angles**: For intimidating or powerful subjects
- **High Angles**: For vulnerability or overview of situations
- **POV Shots**: For windows and confrontations

## Atmospheric Elements

### Environmental Details
- **Dust Particles**: Visible in light beams
- **Moisture**: Rain on windows, dripping water in basement
- **Decay**: Peeling paint, rust, worn materials
- **Texture**: Rough stone, old wood, aged paper

### Atmospheric Effects
- **Fog/Haze**: Adds depth and mystery
- **Vignetting**: Draws focus to center
- **Lens Effects**: Subtle lens flares for supernatural moments
- **Motion Blur**: Used sparingly for action/violence

## Material Qualities

### Common Materials
- **Old Wood**: Dark, worn, rich grain texture
- **Metal**: Tarnished brass, rusted iron, cold steel
- **Paper**: Yellowed, aged, coffee-stained documents
- **Stone**: Rough, damp, moss-covered
- **Glass**: Grimy, streaked, rain-spattered

### Surface Treatment
- Weathered and aged appearance
- Realistic wear and tear
- Muted reflections
- Minimal specular highlights except for key objects

## Scene-Type Visual Guidelines

### Hub/Location Scenes
- Wide establishing shots
- Multiple points of interest visible
- Architectural framing
- Sense of space and navigation

### Investigation/Evidence Scenes
- Close-up macro photography style
- Dramatic overhead or side lighting
- Focus on details and text
- Shallow depth of field effect

### Confrontation/Dialogue Scenes
- Medium shots with character presence
- Dramatic lighting on faces
- Body language emphasis
- Shot-reverse-shot composition suggestions

### Supernatural/Surreal Scenes
- Reality distortion effects
- Vibrant color breakthrough
- Geometric light patterns
- Dreamlike quality while maintaining base realism

### Ending Scenes
- Tailored to emotional resolution
- More vibrant or more muted based on outcome
- Symbolic visual elements
- Sense of finality in composition

## Technical Specifications

### Image Format
- Generated as Texture2D assets
- Stored in Resources/FMV/[GameName]/Images/ (or Resources/FMV/Images/ by default)
- Displayed using Unity's GUI system
- ScaleAndCrop mode for consistent framing

### Aspect Ratio Considerations
- Images displayed in top 40% of screen
- Flexible aspect ratio (adapts to screen)
- Important elements centered for safety
- Text overlays (titles) need readable backgrounds

## Consistency Guidelines

### Maintaining Cohesion
- All scenes share the noir aesthetic foundation
- Lighting always motivated and dramatic
- Color palette restraint except for story reasons
- Material quality consistent across locations

### Visual Progression
- Scenes get progressively more surreal toward supernatural elements
- Endings have distinct visual signatures
- Early game more grounded, late game more abstract
- Color intensity increases at emotional peaks

## Future Expansion

### Video Integration
When transitioning to full video:
- Maintain established lighting and color principles
- Use static images as reference for cinematography
- Preserve composition guidelines for filming
- Keep the noir aesthetic through grading and lighting design

### Additional Scenes
New scenes should:
- Reference existing scene types for consistency
- Follow the established color and lighting rules
- Integrate seamlessly with existing visual language
- Enhance rather than contradict the established mood
