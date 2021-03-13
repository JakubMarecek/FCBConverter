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
        public static Dictionary<uint, string> eventActionSettings = new Dictionary<uint, string>
        {
            {0x00, "VoiceVolume" },
            {0x03, "VoiceLowpassFilter" },
        };
        public static Dictionary<uint, string> eventActionIncluded = new Dictionary<uint, string>
        {
            {0x00, "Embedded" },
            {0x01, "Streamed" },
            {0x02, "StreamedZeroLatency" },
        };
        public static Dictionary<uint, string> eventActionSoundType = new Dictionary<uint, string>
        {
            {0x00, "SoundSFX" },
            {0x01, "SoundVoice" },
        };
        public static Dictionary<uint, string> eventActionAddsParams = new Dictionary<uint, string>
        {
            {0x00, "GeneralSettingsVoiceVolume" },
            {0x02, "GeneralSettingsVoicePitch" },
            {0x03, "GeneralSettingsVoiceLowpassFilter" },
            {0x05, "AdvancedSettingsPlaybackPriorityPriority" },
            {0x06, "AdvancedSettingsPlaybackPriorityOffset" },
            {0x07, "Loop" },
            {0x08, "MotionVolumeOffset" },
            {0x0B, "Positioning2DPannerX" },
            {0x0C, "Positioning2DPannerY" },
            {0x0D, "PositioningCenterPercent" },
            {0x12, "GeneralSettingsUserDefinedAuxiliarySendsBus0Volume" },
            {0x13, "GeneralSettingsUserDefinedAuxiliarySendsBus1Volume" },
            {0x14, "GeneralSettingsUserDefinedAuxiliarySendsBus2Volume" },
            {0x15, "GeneralSettingsUserDefinedAuxiliarySendsBus3Volume" },
            {0x16, "GeneralSettingsGameDefinedAuxiliarySendsVolume" },
            {0x17, "GeneralSettingsOutputBusVolume" },
            {0x18, "GeneralSettingsOutputBusLowpassFilter" },
            {0x3A, "LoopCount" },
        };

        public static Dictionary<uint, string> hircObjects = new Dictionary<uint, string>
        {
            {0x01, "Settings" },
            {0x02, "Sound" },
            {0x03, "EventAction" },
            {0x04, "Event" },
            {0x05, "SequenceContainer" },
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
