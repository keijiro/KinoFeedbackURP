# KinoFeedbackURP

![1](https://github.com/user-attachments/assets/dac080f4-8955-4ef4-9629-b40391b7e711)
![2](https://github.com/user-attachments/assets/953cbc90-6a16-4869-a82d-e00a0cbd5c48)

**KinoFeedbackURP** is a Unity package that provides old-school video feedback
effects for Unity's [Universal Render Pipeline] as a custom [renderer feature].

[Universal Render Pipeline]:
  https://docs.unity3d.com/6000.0/Documentation/Manual/universal-render-pipeline.html

[renderer feature]:
  https://docs.unity3d.com/6000.0/Documentation/Manual/urp/urp-renderer-feature.html

## System Requirements

- Unity 6.0 or newer
- Universal Render Pipeline

## Installation

The KinoFeedbackURP package (`jp.keijiro.kino.feedback.universal`) can be
installed via the "Keijiro" scoped registry using Package Manager. To add the
registry to your project, please follow [these instructions].

[these instructions]:
  https://gist.github.com/keijiro/f8c7e8ff29bfe63d86b888901b82644c

## Setup

1. Add the "Feedback Feature" to an active URP renderer. Refer to the
   [renderer feature] documentation for detailed steps.

2. Add the "Feedback Effect" component to an active camera in a scene. The
   feedback effect is only applied to cameras that have this component.

## Design Notes

- The feedback effect injects the previous frame's image by rendering a
  full-screen quad at the camera's far plane.
- The frame capture occurs just before the post-processing passes, meaning
  post-processing effects do not influence the feedback effect.
- As with any video feedback effect, the result heavily depends on the frame
  rate. Be aware that changes in frame rate will affect the appearance of the
  effect.
