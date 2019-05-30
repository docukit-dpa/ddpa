using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DDPA.Commons.Enums
{
    public class DDPAEnums
    {
        public enum FieldType
        {
            [Display(Name = "Text")]
            TextField = 1,
            [Display(Name = "Numeric")]
            NumericField = 2,
            //DateField = 3,
            //LogicalField = 4,
            [Display(Name = "Memo")]
            MemoField = 5,
            [Display(Name = "Dropdown")]
            ComboField = 6,
            //ListField = 7,
            [Display(Name = "Attachment")]
            Attachment = 8,
            [Display(Name = "Textarea")]
            Textarea = 9,
            [Display(Name = "Checkbox")]
            Checkbox = 10,
        }

        public enum HintListType
        {
            [EnumMember(Value = "STANDARD")]
            STANDARD = 1,

            [EnumMember(Value = "EXISTING ENTRIES")]
            EXISTINGENTRIES = 2,

            [EnumMember(Value = "HIERARCHICAL")]
            HIERARCHICAL = 3,

            [EnumMember(Value = "EXTERNAL USING SQL QUERY")]
            EXTERNALSQLQUERY = 4,

            [EnumMember(Value = "EXTERNAL WITH SOURCE")]
            EXTERNALSOURCE = 5
        }

        public enum Role
        {
            ADMINISTRATOR,
            DPO,
            //AGENCY, //viewing only 
            DEPTHEAD,
            USER
        }

        public enum ErrorCode
        {
            DEFAULT = 0,
            INVALID_INPUT = 100,
            DATA_NOT_FOUND = 200,
            NO_CHANGE = 300,
            NO_AVAILABLE_SPACE = 800,
            EXCEPTION = 999
        }

        public enum Status
        {
            [Display(Name = "Collection")]
            Collection = 1,

            [Display(Name = "Storage")]
            Storage = 2,

            [Display(Name = "Use")]
            Usage = 3,

            [Display(Name = "Disclosure")]
            Transfer = 4,

            [Display(Name = "Retention")]
            Archival = 5,

            [Display(Name = "Disposal")]
            Disposal = 6
        }

        public enum Classification
        {
            [Display(Name = "Non-sensitive")]
            Nonsensitive = 1,

            [Display(Name = "Sensitive")]
            Sensitive = 2
        }

        public enum State
        {
            [Display(Name = "Draft")]
            Draft = 1,

            [Display(Name = "Pending")]
            Pending = 2,

            [Display(Name = "Rework")]
            Rework = 3,

            [Display(Name = "Approved")]
            Approved = 4
        }

        public enum RequestType
        {
            [Display(Name = "Add")]
            Add = 1,

            [Display(Name = "Edit")]
            Edit = 2,

            [Display(Name = "Delete")]
            Delete = 3,

            [Display(Name = "Approved")]
            Approved = 4
        }

        public enum ButtonAction
        {
            [Display(Name = "Save")]
            Save = 1,

            [Display(Name = "Submit")]
            Submit = 2
        }

        public enum TypeOfNotification
        {
            [Display(Name = "Email")]
            Email = 1,

            [Display(Name = "Manual")]
            Manual = 2
        }
    }
}
