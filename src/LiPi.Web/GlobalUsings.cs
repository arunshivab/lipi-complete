// PR2 — Phase 2.8 overlay-cluster migration.
// The overlay subsystem (Modal/Drawer/Toast/DynamicTabs + shared services) moved
// out of LiPi.Web.Services / LiPi.Web.Components.Shared into LiPi.Components.Overlays.
// This global using lets every LiPi.Web .cs / .razor.cs consumer resolve the moved
// service interfaces + types (ILipiModalService, ILipiToastService, DynamicTabInfo,
// ModalSize, etc.) without a per-file `using` edit. The .razor markup side is covered
// by the matching `@using LiPi.Components.Overlays` in Components/_Imports.razor.
global using LiPi.Components.Overlays;
global using LiPi.Components.Feedback;  // LipiSpinner migrated here (PR2)
