namespace Nebula
{
    public static class ErrorKeys
    {
        public const string NotImplemented = "NOT_IMPLEMENTED";
        public const string UnknownInternalServerError = "UNKNOWN_INTERNAL_SERVER_ERROR";

        public const string InputCanNotBeEmpty = "INPUT_CANNOT_BE_EMPTY";
        public const string DuplicateTagInstruction = "DUPLICATE_TAG_INSTRUCTION";
        public const string DuplicateFieldInstruction = "DUPLICATE_FIELD_INSTRUCTION";

        public const string ArgumentCanNotBeEmpty = "ARGUMENT_CANNOT_BE_EMPTY";
        public const string ArgumentCanNotBeNegative = "ARGUMENT_CANNOT_BE_NEGATIVE";
        public const string ArgumentIsSmallerThanMinimum = "ARGUMENT_IS_SMALLER_THAN_MINIMUM";
        public const string ArgumentIsLargerThanMaximum = "ARGUMENT_IS_LARGER_THAN_MAXIMUM";
        public const string ArgumentLengthExceedsMaximum = "ARGUMENT_LENGTH_EXCEEDS_MAXIMUM";
        public const string InvalidCharactersInArgument = "INVALID_CHARACTERS_IN_ARGUMENT";
        public const string ArgumentIsNotANumber = "ARGUMENT_IS_NOT_A_NUMBER";
        public const string InfinityIsNotAllowed = "INFINITY_IS_NOT_ALLOWED";


        public const string TagHierarchyIsTooDeep = "TAG_HIERARCHY_IS_TOO_DEEP";
        public const string TagHierarchyNodeCanNotBeEmpty = "TAG_HIERARCHY_NODE_CANNOT_BE_EMPTY";
        public const string TagHierarchyNodeLengthExceedsMaximum = "TAG_HIERARCHY_NODE_LENGTH_EXCEEDS_MAXIMUM";
        public const string TagHierarchyNodeFormatNotValid = "TAG_HIERARCHY_NODE_FORMAT_NOT_VALID";

        public const string TagNamespaceIsNotDefined = "TAG_NAMESPACE_IS_NOT_DEFINED";
        public const string FieldIsNotDefined = "FIELD_IS_NOT_DEFINED";

        public const string NoPushTargetSpecified = "NO_PUSH_TARGET_SPECIFIED";
        public const string TooManyPushTargetsSpecified = "TOO_MANY_PUSH_TARGETS_SPECIFIED";

        public const string TargetUriIsInvalid = "TARGET_URI_IS_INVALID";
        public const string TargetUriSchemeIsInvalid = "TARGET_URI_SCHEME_IS_INVALID";

        public const string QueryNestingIsTooDeep = "QUERY_NESTING_IS_TOO_DEEP";
        public const string QueryHasTooManyConditions = "QUERY_HAS_TOO_MANY_CONDITIONS";

        public const string InvalidJobId = "INVALID_JOB_ID";
        public const string InvalidJobAction = "INVALID_JOB_ACTION";
        public const string InvalidJobState = "INVALID_JOB_STATE";
        public const string JobActionHasPreprocessorDependency = "JOB_ACTION_HAS_PREPROCESSOR_DEPENDENCY";
    }
}