using System.Collections.Generic;

namespace FCBConverter
{
    class BNKNames
    {
        // https://www.audiokinetic.com/library/edge/?source=Help&id=event_actions_list
        // https://www.audiokinetic.com/library/edge/?source=Help&id=sound_property_editor

        public static Dictionary<uint, string> eventActionScopes = new Dictionary<uint, string>
        {
            {0x01, "GameObjectSwitchOrTrigger" },
            {0x02, "Global" },
            {0x03, "GameObjectRefID" },
            {0x04, "GameObjectState" },
            {0x05, "All" },
            {0x09, "AllExceptRefID" }
        };
        public static Dictionary<uint, string> eventActionTypes = new Dictionary<uint, string>
        {
            {0x01, "Stop" },
            {0x02, "Pause" },
            {0x03, "Resume" },
            {0x04, "Play" },
            {0x05, "Trigger" },
            {0x06, "Mute" },
            {0x07, "UnMute" },
            {0x08, "SetVoicePitch" },
            {0x09, "ResetVoicePitch" },
            {0x0A, "SetVoiceVolume" },
            {0x0B, "ResetVoiceVolume" },
            {0x0C, "SetBusVolume" },
            {0x0D, "ResetBusVolume" },
            {0x0E, "SetVoiceLowpassFilter" },
            {0x0F, "ResetVoiceLowpassFilter" },
            {0x10, "EnableState" },
            {0x11, "DisableState" },
            {0x12, "SetState" },
            {0x13, "SetGameParameter" },
            {0x14, "ResetGameParameter" },
            {0x19, "SetSwitch" },
            {0x1A, "EnableBypassOrDisableBypass" },
            {0x1B, "ResetBypassEffect" },
            {0x1C, "Break" },
            {0x1E, "Seek" },
        };
        public static Dictionary<uint, string> eventActionParams = new Dictionary<uint, string>
        {
            {0x0E, "Delay" },
            {0x0F, "Play" },
            {0x10, "Probability" }
        };
        public static Dictionary<uint, string> rtpcShape = new Dictionary<uint, string>
        {
            {0x00, "LogarithmicBase3" },
            {0x01, "SineConstantPowerFadeIn" },
            {0x02, "LogarithmicBase1.41" },
            {0x03, "InvertedSCurve" },
            {0x04, "Linear" },
            {0x05, "SCurve" },
            {0x06, "ExponentialBase1.41" },
            {0x07, "SineConstantPowerFadeOut" },
            {0x08, "ExponentialBase3" },
            {0x09, "Constant" },
        };
        public static Dictionary<uint, string> eventActionSettings = new Dictionary<uint, string>
        {
            {0x00, "VoiceVolume" },
            {0x03, "VoiceLowpassFilter" },
            {0x02, "VoicePitch" },
            {0x04, "VoiceHighPassFilter" },
            {0x06, "InitialDelay" },
            {0x07, "MakeupGain" },
            {0x0B, "Transposition" },
            {0x0C, "VelocityOffset" },
            {0x11, "Priority" },
            {0x1D, "BypassEffect0" },
            {0x1E, "BypassEffect1" },
            {0x1F, "BypassEffect2" },
            {0x20, "BypassEffect3" },
            {0x21, "BypassAllEffects" },
            {0x26, "GameDefinedAuxiliarySendsVolume" },
            {0x27, "UserDefinedAuxiliarySendVolume0" },
            {0x28, "UserDefinedAuxiliarySendVolume1" },
            {0x29, "UserDefinedAuxiliarySendVolume2" },
            {0x2A, "UserDefinedAuxiliarySendVolume3" },
            {0x2B, "OutputBusVolume" },
            {0x2C, "OutputBusHighPassFilter" },
            {0x2D, "OutputBusLowPassFilter" },
            {0x2F, "UserDefinedAuxiliaryLPF0" },
            {0x30, "UserDefinedAuxiliaryLPF1" },
            {0x31, "UserDefinedAuxiliaryLPF2" },
            {0x32, "UserDefinedAuxiliaryLPF3" },
            {0x33, "UserDefinedAuxiliaryHPF0" },
            {0x34, "UserDefinedAuxiliaryHPF1" },
            {0x35, "UserDefinedAuxiliaryHPF2" },
            {0x36, "UserDefinedAuxiliaryHPF3" },
            {0x37, "GameDefinedAuxiliarySendsLPF" },
            {0x38, "GameDefinedAuxiliarySendsHPF" },
        };
        public static Dictionary<uint, string> eventActionIncluded = new Dictionary<uint, string>
        {
            {0x00, "Embedded" },
            {0x02, "Streamed" },
            {0x01, "StreamedZeroLatency" },
        };
        public static Dictionary<uint, string> eventActionSoundType = new Dictionary<uint, string>
        {
            {0x00, "SoundSFX" },
            {0x01, "SoundVoice" },
            {0x08, "NonCachable" },
        };
        public static Dictionary<uint, string> eventActionAddsParams = new Dictionary<uint, string>
        {
            {0x00, "GeneralSettingsVoiceVolume" },
            {0x02, "GeneralSettingsVoicePitch" },
            {0x03, "GeneralSettingsVoiceLowpassFilter" },
            {0x04, "GeneralSettingsVoiceHighpassFilter" },
            {0x06, "SourceSettingsMakeupGain" },
            {0x07, "AdvancedSettingsPlaybackPriorityPriority" },
            {0x08, "AdvancedSettingsPlaybackPriorityOffsetPriorityBy" },
            {0x0B, "Positioning2DPannerX" },
            {0x0C, "Positioning2DPannerY" },
            {0x0E, "PositioningCenterPercent" },
            {0x13, "GeneralSettingsUserDefinedAuxiliarySendsBus0Volume" },
            {0x14, "GeneralSettingsUserDefinedAuxiliarySendsBus1Volume" },
            {0x15, "GeneralSettingsUserDefinedAuxiliarySendsBus2Volume" },
            {0x16, "GeneralSettingsUserDefinedAuxiliarySendsBus3Volume" },
            {0x17, "GeneralSettingsGameDefinedAuxiliarySendsVolume" },
            {0x18, "GeneralSettingsOutputBusVolume" },
            {0x1A, "GeneralSettingsOutputBusLowpassFilter" },
            {0x19, "GeneralSettingsOutputBusHighpassFilter" },
            {0x3A, "LoopCount" },
            {0x3B, "GeneralSettingsInitialDelay" },
            {0x3C, "GeneralSettingsUserDefinedAuxiliaryLPF0" },
            {0x3D, "GeneralSettingsUserDefinedAuxiliaryLPF1" },
            {0x3E, "GeneralSettingsUserDefinedAuxiliaryLPF2" },
            {0x3F, "GeneralSettingsUserDefinedAuxiliaryLPF3" },
            {0x40, "GeneralSettingsUserDefinedAuxiliaryHPF0" },
            {0x41, "GeneralSettingsUserDefinedAuxiliaryHPF1" },
            {0x42, "GeneralSettingsUserDefinedAuxiliaryHPF2" },
            {0x43, "GeneralSettingsUserDefinedAuxiliaryHPF3" },
        };

        public static Dictionary<uint, string> hircObjects = new Dictionary<uint, string>
        {
            {0x01, "Settings" },
            {0x02, "Sound" },
            {0x03, "EventAction" },
            {0x04, "Event" },
            {0x05, "RandomOrSequenceContainer" },
            {0x06, "SwitchContainer" },
            {0x07, "ActorMixer" },
            {0x08, "AudioBus" },
            {0x09, "BlendContainer" },
            {0x0A, "MusicSegment" },
            {0x0B, "MusicTrack" },
            {0x0C, "MusicSwitchContainer" },
            {0x0D, "MusicPlaylistContainer" },
            {0x0E, "Attenuation" },
            {0x0F, "DialogueEvent" },
            {0x10, "MotionBus" },
            {0x11, "MotionFX" },
            {0x12, "Effect" },
            {0x14, "AuxiliaryBus" },
        };
        public static Dictionary<uint, string> sectionsNames = new Dictionary<uint, string>
        {
            {0x44484B42, "BKHD_BankHeader" },
            {0x58444944, "DIDX_DataIndex" },
            {0x41544144, "DATA_Data" },
            {0x53564E45, "ENVS_Environments" },
            {0x52505846, "FXPR_EffectsProduction" },
            {0x43524948, "HIRC_Hierarchy" },
            {0x44495453, "STID_SoundTypeID" },
            {0x474D5453, "STMG_SoundTypeManager" },
        };
    }
}
