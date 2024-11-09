using System.Text.RegularExpressions;

using GSDML = ISO15745.GSDML;

namespace GsdmlLinker.Core.PN.Models.Manufacturers;

public record IfmDeviceV2(string filePath, Match? match) : IfmDevice(filePath, match)
{
    public const string AL140x = "0xAC6F";

    public override string ProfileParameterIndex => "47360";    // Profile Index=0xB900 (47360)

    public override uint VendorIdSubIndex => 10;
    public override uint DeviceIdSubIndex => 12;

    //internal override GSDML.DeviceProfile.ParameterRecordDataT BuildPortParameters(Models.IOL.Device device) =>
    //    new()
    //    {
    //        Index = "47360",
    //        Length = 16,
    //        TransferSequence = 1,
    //        Name = new GSDML.Primitives.ExternalTextRefT { TextId = "T_IOLD_Port_parameters" },
    //        Items =
    //        [
    //            new GSDML.DeviceProfile.RecordDataConstT
    //            {
    //                Data = "0x01,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00"
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ByteOffset = 3,
    //                BitOffset = 0,
    //                TextId = "T_Enable_Port_Diagnosis",
    //                ID = "ID_PNPC_Bit_0",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
    //                DefaultValue = "1"
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ByteOffset = 3,
    //                BitOffset = 1,
    //                TextId = "T_Enable_Process_Alarm",
    //                ID = "ID_PNPC_Bit_1",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
    //                DefaultValue = "1",
    //                Changeable = false
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ByteOffset = 3,
    //                BitOffset = 2,
    //                TextId = "T_PortConfigSource",
    //                ID = "ID_PNPC_Bit_2",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
    //                DefaultValue = "1"
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ByteOffset = 3,
    //                BitOffset = 3,
    //                TextId = "T_Enable_Input_fraction",
    //                ID = "ID_PNPC_Bit_3",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
    //                DefaultValue = "0",
    //                Changeable = false
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ByteOffset = 3,
    //                BitOffset = 4,
    //                TextId = "T_Enable_Pull_Plug",
    //                ID = "ID_PNPC_Bit_4",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Bit,
    //                DefaultValue = "1"
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ByteOffset = 4,
    //                TextId = "T_hidden",
    //                ID = "ID_PNPC_PDin_Length",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
    //                DefaultValue = "0",
    //                Changeable = false,
    //                Visible = false
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ByteOffset = 5,
    //                TextId = "T_hidden",
    //                ID = "ID_PNPC_PDout_Length",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
    //                DefaultValue = "2",
    //                Changeable = false,
    //                Visible = false
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ValueItemTarget = "V_PortModeIOL",
    //                ByteOffset = 6,
    //                TextId = "T_PortModeIOL",
    //                ID = "ID_PortModeIOL",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
    //                DefaultValue = "1",
    //                AllowedValues = "1",
    //                Changeable = false
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ValueItemTarget = "V_Validation_Backup",
    //                ByteOffset = 7,
    //                TextId = "T_Validation_Backup",
    //                ID = "ID_Validation_Backup",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
    //                DefaultValue = device.Version switch
    //                {
    //                    IODD.ProfileRevision.Revision10 => "1",
    //                    IODD.ProfileRevision.Revision11 => "2",
    //                    _ => "0"
    //                },
    //                AllowedValues = "2",
    //                Changeable = false
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ValueItemTarget = "V_PNPC_PortIQ_Behaviour",
    //                ByteOffset = 8,
    //                TextId = "T_PNPC_PortIQ_Behaviour",
    //                ID = "ID_PNPC_PortIQ_Behaviour",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
    //                DefaultValue = "0",
    //                Changeable = false,
    //                Visible = false
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ValueItemTarget = "V_MasterCycleTime",
    //                ByteOffset = 9,
    //                TextId = "T_MasterCycleTime",
    //                ID = "ID_MasterCycleTime",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned8,
    //                DefaultValue = "0",
    //                AllowedValues = "0 20 40 68 88 128 148 188"
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ByteOffset = 10,
    //                TextId = "T_Validation_VendorID",
    //                ID = "ID_Validation_VendorID",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned16,
    //                DefaultValue = $"{device.Description.VendorId}",
    //                AllowedValues = $"{device.Description.VendorId}",
    //                Changeable = false
    //            },
    //            new GSDML.DeviceProfile.RecordDataRefT
    //            {
    //                ByteOffset = 12,
    //                TextId = "T_Validation_DeviceID",
    //                ID = "ID_Validation_DeviceID",
    //                DataType = GSDML.Primitives.RecordDataRefTypeEnumT.Unsigned32,
    //                DefaultValue = $"{device.Description.DeviceId}",
    //                AllowedValues = $"{device.Description.DeviceId}",
    //                Changeable = false
    //            }
    //        ],
    //        MenuList =
    //        [
    //            new GSDML.DeviceProfile.MenuItemT
    //            {
    //                ID = "MenuItemPort_parameters",
    //                Name = new GSDML.Primitives.ExternalTextRefT { TextId = "T_IOLD_Port_parameters" },
    //                Items =
    //                [
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_Bit_0" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_Bit_1" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_Bit_2" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_Bit_3" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_Bit_4" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_PDin_Length" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_PDout_Length" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PortModeIOL" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_Validation_Backup" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_PNPC_PortIQ_Behaviour" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_Validation_VendorID" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_Validation_DeviceID" },
    //                    new GSDML.DeviceProfile.ParameterRefT { ParameterTarget = "ID_MasterCycleTime" }
    //                ]
    //            }
    //        ]
    //    };

    internal new GSDML.DeviceProfile.ParameterRecordDataT BuildSafeRecord() =>
        new()
        {
            Index = "300",
            Length = 1,
            TransferSequence = 2,
            Name = new GSDML.Primitives.ExternalTextRefT { TextId = "T_FailSafeRecord" },
            Items =
            [
                new GSDML.DeviceProfile.RecordDataConstT { Data = "0x00" },
                new GSDML.DeviceProfile.RecordDataRefT
                {
                    ValueItemTarget = "VAL_FailSafeMode",
                    ByteOffset = 0,
                    BitLength = "8",
                    TextId = "RecFSM_FailSafeMode",
                    DataType = GSDML.Primitives.RecordDataRefTypeEnumT.BitArea,
                    DefaultValue = "0",
                    AllowedValues = "0",
                    Changeable = false
                }
            ]
        };

    internal new GSDML.DeviceProfile.IODataTDataItem PQIBuid() =>
        new()
        {
            DataType = GSDML.Primitives.DataItemTypeEnumT.Unsigned8,
            UseAsBits = true,
            TextId = "PQI",
            BitDataItem =
            [
                new() { BitOffset = 0, TextId = "RE" },
                new() { BitOffset = 1, TextId = "RE" },
                new() { BitOffset = 2, TextId = "NP" },
                new() { BitOffset = 3, TextId = "SV" },
                new() { BitOffset = 4, TextId = "PO" },
                new() { BitOffset = 5, TextId = "DC" },
                new() { BitOffset = 6, TextId = "DE" },
                new() { BitOffset = 7, TextId = "PQ" },
            ]
        };
}
