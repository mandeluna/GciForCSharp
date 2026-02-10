namespace CCKInf2U.Interop;

internal static class GciOop
{
	public const Oop OOP_TAG_SMALLINT = 0x2UL; /* 2r010  SmallInteger */
	public const Oop OOP_TAG_SMALLDOUBLE = 0x6UL; /* 2r110  SmallDouble */
	public const Oop OOP_TAG_SPECIAL = 0x4UL; /* 2r100  true,false,nil, Char, JISChar */

	public const Oop OOP_ILLEGAL = 0x01UL;
	public const Oop OOP_NIL = 0x14UL;
	public const Oop OOP_REMOTE_NIL = 0x114UL;

	// additional instances of UndefinedObject used within the VM, in oop.ht 

	public const Oop OOP_FALSE = 0x0CUL;
	public const Oop OOP_TRUE = 0x10CUL;
	public const Oop OOP_ASCII_NUL = 0x1CUL;
	public const Oop OOP_FIRST_JIS_CHAR = 0x24UL;

	/* OOP_NO_CONTEXT is used to tell GCI calls that do a execute from
	 * a specified context that no context should be used.
	 */
	public const Oop OOP_NO_CONTEXT = OOP_ILLEGAL;

	/* OOP_OVERLAY is used to tell GCI store calls that a store of this
	 * oop should not overwrite an existing value but instead use
	 * whatever value already exists. If no value exists then OOP_NIL
	 * will be stored.
	 */
	public const Oop OOP_OVERLAY = OOP_ILLEGAL;

	public const Oop OOP_CLASS_ARRAY = 66817UL;/* v1.1 oop  523 */
	public const Oop OOP_CLASS_ASSOCIATION = 67073UL;/* v1.1 oop  525 */
	public const Oop OOP_CLASS_IDENTITY_BAG = 67329UL;/* v1.1 oop  527 */
	public const Oop OOP_CLASS_BEHAVIOR = 67585UL;/* v1.1 oop  529 */

	public const Oop OOP_CLASS_BOOLEAN = 68097UL;/* v1.1 oop  533 */
	public const Oop OOP_CLASS_CHARACTER = 68353UL;/* v1.1 oop  535 */
	public const Oop OOP_CLASS_CLASS = 68609UL;/* v1.1 oop  537 */
	public const Oop OOP_CLASS_COLLECTION = 68865UL;/* v1.1 oop  539 */
	public const Oop OOP_CLASS_OBSOLETE_COMPILED_METHOD = 69121UL;/* v1.1 oop  541 */
	public const Oop OOP_CLASS_OBSOLETE_DATE_TIME = 69377UL;/* v1.1 oop  543 */

	public const Oop OOP_CLASS_OBSOLETE_DATE_TIME50 = 101121UL;/* v1.1 oop  791 */

	public const Oop OOP_CLASS_OBSOLETE_DICTIONARY = 69633UL;/* v1.1 oop  545 */
	public const Oop OOP_CLASS_DECIMAL_FLOAT = 69889UL;/* v1.1 oop  547 */
	public const Oop OOP_CLASS_INTEGER = 70145UL;/* v1.1 oop  549 */
	public const Oop OOP_CLASS_INVARIANT_ARRAY = 70401UL;/* v1.1 oop  551 */
	public const Oop OOP_CLASS_INVARIANT_STRING = 70657UL;/* v1.1 oop  553 */
	public const Oop OOP_CLASS_OBSOLETE_LANGUAGE_DICTIONARY = 70913UL;/* v1.1 oop  555 */
	public const Oop OOP_CLASS_MAGNITUDE = 71169UL;/* v1.1 oop  557 */
	public const Oop OOP_CLASS_ObsoleteMetaclass = 71425UL;/* v1.1 oop  559 */

	public const Oop OOP_CLASS_NUMBER = 71937UL;/* v1.1 oop  563 */
	public const Oop OOP_CLASS_OBJECT = 72193UL;/* v1.1 oop  565 */
	public const Oop OOP_CLASS_POSITIONABLE_STREAM = 72449UL;/* v1.1 oop  567 */
	public const Oop OOP_CLASS_READ_STREAM = 72705UL;/* v1.1 oop  569 */
	public const Oop OOP_OLD_CLASS_REPOSITORY = 72961UL;/* v1.1 oop  571 , pre-v2.2 class */
	public const Oop OOP_CLASS_GsObjectSecurityPolicy = 73217UL;/* v1.1 oop  573 */
	public const Oop OOP_CLASS_SEGMENT = 73217UL;/* old name */

	public const Oop OOP_CLASS_SEQUENCEABLE_COLLECT = 73729UL;/* v1.1 oop  577 */
	public const Oop OOP_CLASS_IDENTITY_SET = 73985UL;/* v1.1 oop  579 */
	public const Oop OOP_CLASS_SMALL_INTEGER = 74241UL;/* v1.1 oop  581 */
	public const Oop OOP_CLASS_STREAM = 74497UL;/* v1.1 oop  583 */
	public const Oop OOP_CLASS_STRING = 74753UL;/* v1.1 oop  585 */
	public const Oop OOP_CLASS_OBSOLETE_SYMBOL = 75009UL;/* v1.1 oop  587 */
	public const Oop OOP_CLASS_OBSOLETE_SYMBOL_ASSOCIATION = 75265UL;/* v1.1 oop  589 */
	public const Oop OOP_CLASS_OBSOLETE_SYMBOL_DICTIONARY = 75521UL;/* v1.1 oop  591 */
	public const Oop OOP_CLASS_OBSOLETE_SYMBOL_SET = 75777UL;/* v1.1 oop  593 */
	public const Oop OOP_CLASS_SYSTEM = 76033UL;/* v1.1 oop  595 */
	public const Oop OOP_CLASS_UNDEFINED_OBJECT = 76289UL;/* v1.1 oop  597 */
	public const Oop OOP_CLASS_USER_PROFILE = 76545UL;/* v1.1 oop  599 */
	public const Oop OOP_CLASS_ABS_USERPROFILE_SET = 76801UL;/* v1.1 oop  601 */
	public const Oop OOP_CLASS_WRITE_STREAM = 77057UL;/* v1.1 oop  603 */
	public const Oop OOP_CLASS_OBS_LARGE_POSITIVE_INT = 77313UL;/* v1.1 oop  605 */
	public const Oop OOP_CLASS_OBS_LARGE_NEGATIVE_INT = 77569UL;/* v1.1 oop  607 */
	public const Oop OOP_CLASS_FRACTION = 77825UL;/* v1.1 oop  609 */
	public const Oop OOP_CLASS_OBSOLETE_CLAMP_SPEC = 78081UL;/* v1.1 oop  611 */
	public const Oop OOP_CLASS_OBSOLETE_IDENTITY_DICTIONARY = 78337UL;/* v1.1 oop  613 */
	public const Oop OOP_CLASS_SYMBOL_LIST = 78593UL;/* v1.1 oop  615 */

	public const Oop OOP_CLASS_ABSTRACT_COLLISION_BUCKET = 79105UL;/* v1.1 oop  619 */
	public const Oop OOP_CLASS_KEY_VALUE_DICTIONARY = 79361UL;/* v1.1 oop  621 */
	public const Oop OOP_CLASS_INT_KEY_VALUE_DICTIONARY = 79617UL;/* v1.1 oop  623 */
	public const Oop OOP_CLASS_STR_KEY_VALUE_DICTIONARY = 79873UL;/* v1.1 oop  625 */
	public const Oop OOP_CLASS_OBSOLETE_SYM_KEY_VALUE_DICTIONARY = 80129UL;/* v1.1 oop  627 */
	public const Oop OOP_CLASS_CHARACTER_COLLECTION = 80385UL;/* v1.1 oop  629 */
	public const Oop OOP_CLASS_JAPANESE_STRING = 80641UL;/* v1.1 oop  631 */
	public const Oop OOP_CLASS_EUC_STRING = 80897UL;/* v1.1 oop  633 */
	public const Oop OOP_CLASS_INVARIANT_EUC_STRING = 81153UL;/* v1.1 oop  635 */
	public const Oop OOP_CLASS_EUC_SYMBOL = 81409UL;/* v1.1 oop  637 */
	public const Oop OOP_CLASS_ABSTRACT_CHARACTER = 81665UL;/* v1.1 oop  639 */
	public const Oop OOP_CLASS_JIS_CHARACTER = 81921UL;/* v1.1 oop  641 */
	/* following added in 3.2 */
	public const Oop OOP_CLASS_CLUSTER_BUCKET = 82177UL;/* v1.1 oop  643 */
	public const Oop OOP_CLASS_CLUSTER_BUCKET_ARRAY = 82433UL;/* v1.1 oop  645 */
	public const Oop OOP_CLASS_HISTORY = 82689UL;/* v1.1 oop  647 */
	public const Oop OOP_CLASS_RCQUEUE = 82945UL;/* v1.1 oop  649 */

	/* added in 4.0 */
	public const Oop OOP_CLASS_OBS_STACK_SEGMENT = 83201UL;/* v1.1 oop  651 */
	public const Oop OOP_CLASS_OBS_STACK_BUFFER = 83457UL;/* v1.1 oop  653 */
	public const Oop OOP_CLASS_OBS_ACTIVATION = 83713UL;/* v1.1 oop  655 */
	public const Oop OOP_CLASS_OBSOLETE_PROCESS = 83969UL;/* v1.1 oop  657 */
	public const Oop OOP_CLASS_oldVARIABLE_CONTEXT = 84225UL;/* v1.1 oop  659 */
	public const Oop OOP_CLASS_BLOCK_CLOSURE = 84481UL;/* v1.1 oop  661 */

	// deletion
	public const Oop OOP_CLASS_oldEXECUTABLE_BLOCK = 84737UL;/* v1.1 oop  663 */
	public const Oop OOP_CLASS_oldSIMPLE_BLOCK = 84993UL;/* v1.1 oop  665 */
	public const Oop OOP_CLASS_oldCOMPLEX_BLOCK = 85249UL;/* v1.1 oop  667 */
	public const Oop OOP_CLASS_oldCOMPLEX_VC_BLOCK = 85505UL;/* v1.1 oop  669 */
	public const Oop OOP_CLASS_SELECT_BLOCK = 85761UL;/* v1.1 oop  671 */
	// deletion

	public const Oop OOP_CLASS_BTREE_NODE = 86017UL;/* v1.1 oop  673 */
	public const Oop OOP_CLASS_BTREE_INTERIOR_NODE = 86273UL;/* v1.1 oop  675 */
	public const Oop OOP_CLASS_BTREE_LEAF_NODE = 86529UL;/* v1.1 oop  677 */
	public const Oop OOP_CLASS_BTREE_BASIC_INTERIOR_NODE = 86785UL;/* v1.1 oop  679 , index.c is order dependent */
	public const Oop OOP_CLASS_BTREE_BASIC_LEAF_NODE = 87041UL;/* v1.1 oop  681 */

	public const Oop OOP_CLASS_RCKEY_VALUE_DICTIONARY = 87297UL;/* v1.1 oop  683 */
	public const Oop OOP_CLASS_OBSOLETE_RC_COLLISION_BUCKET = 87553UL;/* v1.1 oop  685 */
	public const Oop OOP_CLASS_RCINDEX_DICTIONARY = 87809UL;/* v1.1 oop  687 */
	public const Oop OOP_CLASS_RCINDEX_BUCKET = 88065UL;/* v1.1 oop  689 */
	public const Oop OOP_CLASS_BUCKET_VALUE_BAG = 88321UL;/* v1.1 oop  691 */

	public const Oop OOP_CLASS_IDENTITY_INDEX = 88577UL;/* v1.1 oop  693 */
	public const Oop OOP_CLASS_RANGE_EQUALITY_INDEX = 88833UL;/* v1.1 oop  695 */
	public const Oop OOP_CLASS_PATH_TERM = 89089UL;/* v1.1 oop  697 */
	public const Oop OOP_CLASS_INDEX_LIST = 89345UL;/* v1.1 oop  699 */
	public const Oop OOP_CLASS_DEPENDENCY_LIST = 89601UL;/* v1.1 oop  701 */
	public const Oop OOP_CLASS_SET_VALUED_PATH_TERM = 89857UL;/* v1.1 oop  703 */
	public const Oop OOP_CLASS_MAPPING_INFO = 90113UL;/* v1.1 oop  705 */
	public const Oop OOP_CLASS_CONSTRAINED_PATH_TERM = 90369UL;/* v1.1 oop  707 */
	public const Oop OOP_CLASS_QUERY_EXECUTER = 90625UL;/* v1.1 oop  709 */
	public const Oop OOP_CLASS_IDENTITY_KEY_VALUE_DICTIONARY = 90881UL;/* v1.1 oop  711 */
	public const Oop OOP_CLASS_OBSOLETE_IDENTITY_COLLISION_BUCKET = 91137UL;/* v1.1 oop  713 */

	public const Oop OOP_CLASS_OBSOLETE_BTREE_READSTREAM = 91393UL;/* v1.1 oop  715 */
	public const Oop OOP_CLASS_OBSOLETE_RANGE_INDEX_READSTREAM = 91649UL;/* v1.1 oop  717 */
	public const Oop OOP_CLASS_PATH_EVALUATOR = 91905UL;/* v1.1 oop  719 */
	public const Oop OOP_CLASS_LOG_ENTRY = 92161UL;/* v1.1 oop  721 */

	/* DB conversion note: the following classes exist WITHOUT reserved
	 *  oops in 3.2 Geode databases.  Oops of the such classes and 
	 *  references to the classes must be changed by the fullbackup phase
	 *  of 3.2 to 4.0 database conversion
	 */
	public const Oop OOP_CLASS_OBSOLETE_GEODE = 92417UL;/* v1.1 oop  723 */
	public const Oop OOP_CLASS_ORDERED_COLLECTION = 92673UL;/* v1.1 oop  725 */
	public const Oop OOP_CLASS_SORTED_COLLECTION = 92929UL;/* v1.1 oop  727 */
	public const Oop OOP_CLASS_CLASS_ORGANIZER = 93185UL;/* v1.1 oop  729 */
	public const Oop OOP_CLASS_PROF_MONITOR = 93441UL;/* v1.1 oop  731 */
	public const Oop OOP_CLASS_CLASS_SET = 93697UL;/* v1.1 oop  733 */
	public const Oop OOP_CLASS_AUTO_COMPLETE = 93953UL;/* v1.1 oop  735 */
	public const Oop OOP_CLASS_STRING_PAIR = 94209UL;/* v1.1 oop  737 */
	public const Oop OOP_CLASS_STRING_PAIR_SET = 94465UL;/* v1.1 oop  739 */

	public const Oop OOP_CLASS_OBSOLETE_SYMBOL_LIST_DICTIONARY = 94721UL;/* v1.1 oop  741 */

	public const Oop OOP_CLASS_REDO_LOG = 94977UL;/* v1.1 oop  743 */

	public const Oop OOP_CLASS_GSCLASS_DOCUMENTATION = 95233UL;/* v1.1 oop  745 */
	public const Oop OOP_CLASS_GSDOC_TEXT = 95489UL;/* v1.1 oop  747 */

	public const Oop OOP_CLASS_SORT_NODE = 95745UL;/* v1.1 oop  749 , index.c is order dependent*/
	public const Oop OOP_CLASS_BASIC_SORT_NODE = 96001UL;/* v1.1 oop  751 , index.c is order dependent*/

	public const Oop OOP_CLASS_UNORDERED_COLLECTION = 96257UL;/* v1.1 oop  753 */

	public const Oop OOP_CLASS_BINARY_FLOAT = 96513UL;/* v1.1 oop  755 */
	public const Oop OOP_CLASS_ISO_LATIN = 96769UL;/* v1.1 oop  757 */
	public const Oop OOP_CLASS_GsObjectSecurityPolicySet = 97025UL;/* v1.1 oop  759 */
	public const Oop OOP_CLASS_SEGMENT_SET = 97025UL;/* old name */

	public const Oop OOP_CLASS_OBS_FLOAT = 97281UL;/* v1.1 oop  761 */
	public const Oop OOP_CLASS_OBS_SMALL_FLOAT = 97537UL;/* v1.1 oop  763 */
	public const Oop OOP_CLASS_UNIMPLEMENTED_FLOAT_1 = 97793UL;/* v1.1 oop  765 */
	public const Oop OOP_CLASS_UNIMPLEMENTED_FLOAT_2 = 98049UL;/* v1.1 oop  767 */

	public const Oop OOP_CLASS_JIS_STRING = 98305UL;/* v1.1 oop  769 */
	public const Oop OOP_CLASS_INDEX_BKT_WITH_CACHE = 98561UL;/* v1.1 oop  771 */

	/* following new in 5.0 */

	public const Oop OOP__CLASS_GSMETHOD = 98817UL;/* v1.1 oop  773 */
	public const Oop OOP_CLASS_GSMETHOD_DICTIONARY = 99073UL;/* v1.1 oop  775 */
	public const Oop OOP_CLASS_oldDOUBLE_BYTE_STRING = 99329UL;/* v1.1 oop  777 */
	public const Oop OOP_CLASS_oldDOUBLE_BYTE_SYMBOL = 99585UL;/* v1.1 oop  779 */
	public const Oop OOP_CLASS_oldGSPROCESS = 99841UL;/* v1.1 oop  781 */
	public const Oop OOP_CLASS_GSSTACK_BUFFER = 100097UL;/* v1.1 oop  783 */

	public const Oop OOP_CLASS_CLASS_DEF_INFO = 100353UL;/* v1.1 oop  785 */

	public const Oop OOP_CLASS_DATE = 100609UL;/* v1.1 oop  787 */
	public const Oop OOP_CLASS_TIME = 100865UL;/* v1.1 oop  789 */

	public const Oop OOP_CLASS_N_DICTIONARY = 101377UL;/* v1.1 oop  793 */
	public const Oop OOP_CLASS_N_IDENTITY_DICTIONARY = 101633UL;/* v1.1 oop  795 */
	public const Oop OOP_CLASS_N_LANGUAGE_DICTIONARY = 101889UL;/* v1.1 oop  797 */
	public const Oop OOP_CLASS_MODIFICATION_LOG = 102145UL;/* v1.1 oop  799 */

	public const Oop OOP_CLASS_EQUALITY_SET = 102401UL;/* v1.1 oop  801 */
	public const Oop OOP_CLASS_EQUALITY_BAG = 102657UL;/* v1.1 oop  803 */

	public const Oop OOP_CLASS_FixedPoint = 102913UL;/* v1.1 oop  805 */ /* was ScaledDecimal in v2.4*/
	public const Oop OOP_CLASS_INTERVAL = 103169UL;/* v1.1 oop  807 */
	public const Oop OOP_CLASS_BYTE_ARRAY = 103425UL;/* v1.1 oop  809 */

	/* classes added in 5.0 */
	public const Oop OOP_CLASS_GSSESSION = 103681UL;/* v1.1 oop  811 */
	public const Oop OOP_CLASS_GSCURRENT_SESSION = 103937UL;/* v1.1 oop  813 */
	public const Oop OOP_CLASS_GSREMOTE_SESSION = 104193UL;/* v1.1 oop  815 */
	public const Oop OOP_CLASS_INDEX_DICT_ENTRY_HOLDER = 104449UL;/* v1.1 oop  817 */
	// 41056: obsolete GSRDB_CONNECTION
	public const Oop OOP_CLASS_OBSOLETE_GSRDB_CONNECTION = 104705UL;/* v1.1 oop  819 */
	public const Oop OOP_CLASS_GSINTERSESS_SIGNAL = 104961UL;/* v1.1 oop  821 */
	public const Oop OOP_CLASS_ABSTRACT_SESSION = 105217UL;/* v1.1 oop  823 */

	/* classes that existed in 4.1 without reserved oops */
	public const Oop OOP_CLASS_RCIDENTITY_BAG = 105473UL;/* v1.1 oop  825 */
	/* These next two were reserved but are no longer needed */
	/* was for OOP_CLASS_RCPOS_COUNTER  105729U   v1.1 oop  827 */
	/* was for OOP_CLASS_RCPIPE         105985U   v1.1 oop  829 */
	public const Oop OOP_CLASS_CONSTR_PATH_EVAL = 106241UL;/* v1.1 oop  831 */
	public const Oop OOP_CLASS_SETVAL_PATH_EVAL = 106497UL;/* v1.1 oop  833 */
	public const Oop OOP_CLASS_PATH_SORTER = 106753UL;/* v1.1 oop  835 */
	public const Oop OOP_CLASS_DEP_LIST_BUCKET = 107009UL;/* v1.1 oop  837 */
	public const Oop OOP_CLASS_DEP_LIST_TABLE = 107265UL;/* v1.1 oop  839 */
	public const Oop OOP_CLASS_SORT_NODE_ARRAY = 107521UL;/* v1.1 oop  841 */
	public const Oop OOP_CLASS_NSC_BUILDER = 107777UL;/* v1.1 oop  843 */
	public const Oop OOP_CLASS_PRINT_STREAM = 108033UL;/* v1.1 oop  845 */
	public const Oop OOP_CLASS_RCCOUNTER_ELEMENT = 108289UL;/* v1.1 oop  847 */
	public const Oop OOP_CLASS_RCQUEUE_ELEMENT = 108545UL;/* v1.1 oop  849 */
	public const Oop OOP_CLASS_RCQ_REM_SEQ_NUMS = 108801UL;/* v1.1 oop  851 */
	public const Oop OOP_CLASS_RCQ_SESSION_COMP = 109057UL;/* v1.1 oop  853 */
	public const Oop OOP_CLASS_RCCOUNTER = 109313UL;/* v1.1 oop  855 */

	/* more classes added in 5.0 */
	public const Oop OOP_CLASS_CLIENT_FORWARDER = 109569UL;/* v1.1 oop  857 */
	public const Oop OOP_CLASS_CANON_STR_DICTIONARY_old = 109825UL;/* v1.1 oop  859 */
	public const Oop OOP_CLASS_oldCLAMP_SPECIFICATION = 110081UL;/* v1.1 oop  861 */
	public const Oop OOP_CLASS_OBSOLETE_GS_FILE = 110337UL;/* v1.1 oop  863 */

	public const Oop OOP_CLASS_ABSTRACT_DICTIONARY = 110593UL;/* v1.1 oop  865 */
	public const Oop OOP_CLASS_SYMBOL = 110849UL;/* v1.1 oop  867 */
	public const Oop OOP_CLASS_SYM_KEY_VALUE_DICTIONARY = 111105UL;/* v1.1 oop  869 */
	public const Oop OOP_CLASS_SYMBOL_DICT = 111361UL;/* v1.1 oop  871 */
	public const Oop OOP_CLASS_SYMBOL_ASSOCIATION = 111617UL;/* v1.1 oop  873 */

	public const Oop OOP_CLASS_IDENTITY_BTREE_NODE = 111873UL;/* v1.1 oop  875 */

	public const Oop OOP_CLASS_USERPROFILE_SET = 112129UL;/* v1.1 oop  877 */
	public const Oop OOP_CLASS_USERSECURITY_DATA = 112385UL;/* v1.1 oop  879 */

	public const Oop OOP_CLASS_GS_COMMIT_LIST = 112641UL;/* v1.1 oop  881 */

	/* More classes that did not have reservedOop in 4.1 */
	public const Oop OOP_CLASS_oldGS_SOCKET = 112897UL;/* v1.1 oop  883 */
	public const Oop OOP_CLASS_PASSIVE_OBJECT = 113153UL;/* v1.1 oop  885 */
	public const Oop OOP_CLASS_GS_CLONE_LIST = 113409UL;/* v1.1 oop  887 */

	public const Oop OOP_CLASS_DB_CONVERSION = 113665UL;/* v1.1 oop  889 */
	public const Oop OOP_CLASS_SYMBOL_SET = 113921UL;/* v1.1 oop  891 */

	public const Oop OOP_CLASS_IDENTITY_COLLISION_BUCKET = 114177UL;/* v1.1 oop  893 */
	public const Oop OOP_CLASS_COLLISION_BUCKET = 114433UL;/* v1.1 oop  895 */
	public const Oop OOP_CLASS_RCCOLLISION_BUCKET = 114689UL;/* v1.1 oop  897 */
	public const Oop OOP_CLASS_oldGS_FILE = 114945UL;/* v1.1 oop  899 */

	public const Oop OOP_CLASS_FED_RESERVED_1 = 115201UL;/* v1.1 oop  901 */
	public const Oop OOP_CLASS_FED_RESERVED_2 = 115457UL;/* v1.1 oop  903 */

	/* New 5.1 kernel classes */
	public const Oop OOP_CLASS_SEMAPHORE = 115713UL;/* v1.1 oop  905 */
	public const Oop OOP_CLASS_DELAY = 115969UL;/* v1.1 oop  907 */
	public const Oop OOP_CLASS_PROCESSOR = 116225UL;/* v1.1 oop  909 */
	public const Oop OOP_CLASS_PROCESSOR_SCHEDULER = 116481UL;/* v1.1 oop  911 */
	/*  unused  116737U   v1.1 oop  913 */
	public const Oop OOP_CLASS_SHARED_QUEUE = 116993UL;/* v1.1 oop  915 */
	public const Oop OOP_CLASS_CRITICAL_SECTION = 117249UL;/* v1.1 oop  917 */
	public const Oop OOP_CLASS_GEMSTONE_PARAMETERS = 117505UL;/* v1.1 oop  919 */
	public const Oop OOP_CLASS_ERROR_DESCRIPTION = 117761UL;/* v1.1 oop  921 */
	public const Oop OOP_CLASS_CBUFFER = 118017UL;/* v1.1 oop  923 */
	public const Oop OOP_CLASS_GCI_INTERFACE = 118273UL;/* v1.1 oop  925 */
	/* changed name for TimeZone -- JGF 2007-02-12 */
	public const Oop OOP_CLASS_OBSOLETE_TIME_ZONE = 118529UL;/* v1.1 oop  927 */
	public const Oop OOP_CLASS_DATE_TIME = 118785UL;/* v1.1 oop  929 */

	/* added for Gemstone64 1.0 */
	public const Oop OOP_CLASS_CANON_STRING_BUCKET = 119041UL;/* v1.1 oop  931 */
	public const Oop OOP_CLASS_CANON_STRING_DICT = 119297UL;/* v1.1 oop  933 */
	public const Oop OOP_CLASS_CANON_SYMBOL_DICT = 119553UL;/* v1.1 oop  935 */
	public const Oop OOP_CLASS_FAST_IDENTITY_KEY_VALUE_DICTIONARY = 119809UL;/* v1.1 oop  937 */

	public const Oop OOP_CLASS_SOFTREFERENCE = 120065UL;/* v1.1 oop  939 */
	public const Oop OOP_CLASS_KEY_SOFTVALUE_DICT = 120321UL;/* v1.1 oop  941 */
	public const Oop OOP_CLASS_IDENTITY_KEY_SOFTVALUE_DICT = 120577UL;/* v1.1 oop  943 */
	public const Oop OOP_CLASS_SOFT_COLLISION_BUCKET = 120833UL;/* v1.1 oop  945 */
	public const Oop OOP_CLASS_IDENTITY_SOFT_COLLISION_BUCKET = 121089UL;/* v1.1 oop  947 */

	/* Added in Gemstone64 v2.0 */
	public const Oop OOP_CLASS_SMALL_DOUBLE = 121345UL;/* v1.1 oop  949 */
	public const Oop OOP_CLASS_INDEX_MANAGER = 121601UL;/* v1.1 oop  951 */

	/* Added in Gemstone64 v2.1 */
	public const Oop OOP_CLASS_INDEXED_QUERY_EVALUATOR = 121857UL;/* v1.1 oop  953 */
	public const Oop OOP_CLASS_EQUALITY_INDEX_QUERY_EVALUATOR = 122113UL;/* v1.1 oop  955 */
	public const Oop OOP_CLASS_IDENTITY_INDEX_QUERY_EVALUATOR = 122369UL;/* v1.1 oop  957 */
	public const Oop OOP_CLASS_BTREE_COMPARISON_FOR_SORT = 122625UL;/* v1.1 oop  959 */
	public const Oop OOP_CLASS_BTREE_COMPARISON_FOR_COMPARE = 122881UL;/* v1.1 oop  961 */

	public const Oop OOP_CLASS_INDEX_MANAGER_AUTO_COMMIT_POLICY = 123137UL;/* v1.1 oop  963 */

	public const Oop OOP_CLASS_BTREE_QUERY_SPEC = 123393UL;/* v1.1 oop  965 */
	public const Oop OOP_CLASS_BTREE_COMPARISON_QUERY_SPEC = 123649UL;/* v1.1 oop  967 */
	public const Oop OOP_CLASS_BTREE_RANGE_COMPARISON_QUERY_SPEC = 123905UL;/* v1.1 oop  969 */

	// Locale is used in 6.1.5+ repositories
	public const Oop OOP_CLASS_LOCALE = 124161UL;/* v6.1 oop  1941 */ /* v1.1 oop  971 */

	/* Added in Gemstone64 v2.2 */
	public const Oop OOP_CLASS_REPOSITORY = 124417UL;/* v1.1 oop 973 */

	public const Oop OOP_CLASS_RCQUEUE_ENTRY = 124673UL;/* v1.1 oop 975 */
	public const Oop OOP_CLASS_RC_BTREE_BASIC_INTERIOR_NODE = 124929UL;/* v1.1 oop  977 , index.c is order dependent*/
	public const Oop OOP_CLASS_RC_BTREE_BASIC_LEAF_NODE = 125185UL;/* v1.1 oop  979 */
	public const Oop OOP_CLASS_RC_RANGE_EQUALITY_INDEX = 125441UL;/* v1.1 oop  981 */
	public const Oop OOP_CLASS_RC_BTREE_INTERIOR_NODE = 125697UL;/* v1.1 oop  983 */
	public const Oop OOP_CLASS_RC_BTREE_LEAF_NODE = 125953UL;/* v1.1 oop  985 */

	public const Oop OOP_CLASS_GS_SESS_METH_DICT = 126209UL;/* v1.1 oop  987 */

	// QuadByte string classes from v2.3
	public const Oop OOP_CLASS_oldMULTI_BYTE_STRING = 126465UL;/* v1.1 oop 989 */
	public const Oop OOP_CLASS_oldQUAD_BYTE_STRING = 126721UL;/* v1.1 oop 991 */
	public const Oop OOP_CLASS_oldQUAD_BYTE_SYMBOL = 126977UL;/* v1.1 oop 993 */

	public const Oop OOP_CLASS_Regexp = 127233UL;/* v1.1 oop 995 */
	public const Oop OOP_CLASS_MatchData = 127489UL;/* v1.1 oop 997 */
	public const Oop OOP_CLASS_ExecBlock0 = 127745UL;/* v1.1 oop 999 */
	public const Oop OOP_CLASS_ExecBlock1 = 128001UL;/* v1.1 oop 1001 */
	public const Oop OOP_CLASS_ExecBlock2 = 128257UL;/* v1.1 oop 1003 */
	public const Oop OOP_CLASS_ExecBlock3 = 128513UL;/* v1.1 oop 1005 */
	public const Oop OOP_CLASS_ExecBlock4 = 128769UL;/* v1.1 oop 1007 */
	public const Oop OOP_CLASS_ExecBlock5 = 129025UL;/* v1.1 oop 1009 */
	public const Oop OOP_CLASS_ExecBlockN = 129281UL;/* v1.1 oop 1011 */
	public const Oop OOP_CLASS_Range = 129537UL;/* v1.1 oop 1013 */
	public const Oop OOP_CLASS_IO = 129793UL;/* v1.1 oop 1015 */
	public const Oop OOP_CLASS_ExceptionSet = 130049UL;/* v1.1 oop 1017 */
	public const Oop OOP_CLASS_AbstractException = 130305UL;/* v1.1 oop 1019 */
	public const Oop OOP_CLASS_GsExceptionHandler = 130561UL;/* v1.1 oop 1021 */
	public const Oop OOP_CLASS_Error = 130817UL;
	public const Oop OOP_CLASS_MessageNotUnderstood = 131073UL;
	public const Oop OOP_CLASS_ZeroDivide = 131329UL;
	public const Oop OOP_CLASS_NameError = 131585UL;
	public const Oop OOP_CLASS_InternalError = 131841UL;
	public const Oop OOP_CLASS_CompileError = 132097UL;
	public const Oop OOP_CLASS_LookupError = 132353UL;
	public const Oop OOP_CLASS_UserDefinedError = 132609UL;
	public const Oop OOP_CLASS_ControlInterrupt = 132865UL;
	public const Oop OOP_CLASS_Halt = 133121UL;
	public const Oop OOP_CLASS_Notification = 133377UL;

	public const Oop OOP_CLASS_CLibrary = 133633UL;
	public const Oop OOP_CLASS_CFunction = 133889UL;
	public const Oop OOP_CLASS_CPointer = 134145UL;
	public const Oop OOP_CLASS_CByteArray = 134401UL;
	public const Oop OOP_CLASS_GsProcess = 134657UL;
	public const Oop OOP_CLASS_VariableContext = 134913UL;
	public const Oop OOP_CLASS_GsFile = 135169UL;
	public const Oop OOP_CLASS_GsSocket = 135425UL;
	public const Oop OOP_CLASS_CLAMP_SPECIFICATION = 135681UL;/* v1.1 oop  1061 */
	public const Oop OOP_CLASS_Float = 135937UL;
	public const Oop OOP_CLASS_LargeInteger = 136193UL;
	public const Oop OOP_CLASS_MultiByteString = 136449UL;
	public const Oop OOP_CLASS_SmallFloat = 136705UL;
	public const Oop OOP_CLASS_TrueClass = 136961UL;
	public const Oop OOP_CLASS_FalseClass = 137217UL;
	public const Oop OOP_CLASS_BitSet = 137473UL;
	public const Oop OOP_CLASS_Exception = 137729UL;
	public const Oop OOP_CLASS_GsFileStat = 137985UL;
	public const Oop OOP_CLASS_RubyTime = 138241UL;
	public const Oop OOP_CLASS_RubyHash = 138497UL;
	public const Oop OOP_CLASS_AlmostOutOfMemory = 138753UL;
	public const Oop OOP_CLASS_ArgumentError = 139009UL;
	public const Oop OOP_CLASS_IOError = 139265UL;
	public const Oop OOP_CLASS_RepositoryViewLost = 139521UL;
	public const Oop OOP_CLASS_OffsetError = 139777UL;
	public const Oop OOP_CLASS_OutOfRange = 140033UL;
	public const Oop OOP_CLASS_FloatingPointError = 140289UL;
	public const Oop OOP_CLASS_RegexpError = 140545UL;
	public const Oop OOP_CLASS_SecurityError = 140801UL;
	public const Oop OOP_CLASS_SystemCallError = 141057UL;
	public const Oop OOP_CLASS_ThreadError = 141313UL;
	public const Oop OOP_CLASS_ArgumentTypeError = 141569UL;
	public const Oop OOP_CLASS_AlmostOutOfStack = 141825UL;
	public const Oop OOP_CLASS_CannotReturn = 142081UL;
	public const Oop OOP_CLASS_SocketError = 142337UL;
	public const Oop OOP_CLASS_ImproperOperation = 142593UL;
	public const Oop OOP_CLASS_RubyBreakException = 142849UL;
	public const Oop OOP_CLASS_Module = 143105UL;
	public const Oop OOP_CLASS_EXEC_BLOCK = 143361UL;
	public const Oop OOP_CLASS_GSNATIVECODE = 143617UL;
	public const Oop OOP_CLASS_DoubleByteString = 143873UL;
	public const Oop OOP_CLASS_DoubleByteSymbol = 144129UL;
	public const Oop OOP_CLASS_QuadByteString = 144385UL;
	public const Oop OOP_CLASS_QuadByteSymbol = 144641UL;
	public const Oop OOP_CLASS_GSNMETHOD = 144897UL;
	public const Oop OOP_CLASS_CZstream = 145153UL;
	public const Oop OOP_CLASS_GS_OBJ_INV = 145409UL;
	public const Oop OOP_CLASS_GS_OBJ_INV_ENTRY = 145665UL;
	public const Oop OOP_CLASS_TransientShortArray = 145921UL;
	public const Oop OOP_CLASS_Metaclass3 = 146177UL;
	public const Oop OOP_CLASS_ScaledDecimal = 146433UL;
	public const Oop OOP_CLASS_EqualityCollisionBucket = 146689UL;
	public const Oop OOP_CLASS_CCallout = 146945UL;
	public const Oop OOP_CLASS_CCallin = 147201UL;
	public const Oop OOP_CLASS_RubyThrowException = 147457UL;
	public const Oop OOP_CLASS_Break = 147713UL;
	public const Oop OOP_CLASS_Breakpoint = 147969UL;
	public const Oop OOP_CLASS_ClientForwarderSend = 148225UL;
	public const Oop OOP_CLASS_EndOfStream = 148481UL;
	public const Oop OOP_CLASS_ExternalError = 148737UL;
	public const Oop OOP_CLASS_IndexingErrorPreventingCommit = 148993UL;
	public const Oop OOP_CLASS_GciTransportError = 149249UL;
	public const Oop OOP_CLASS_LockError = 149505UL;
	public const Oop OOP_CLASS_NumericError = 149761UL;
	public const Oop OOP_CLASS_RepositoryError = 150017UL;
	public const Oop OOP_CLASS_TransactionError = 150273UL;
	public const Oop OOP_CLASS_UncontinuableError = 150529UL;
	public const Oop OOP_CLASS_Admonition = 150785UL;
	public const Oop OOP_CLASS_Deprecated = 151041UL;
	public const Oop OOP_CLASS_FloatingPointException = 151297UL;
	public const Oop OOP_CLASS_ObjectsCommittedNotification = 151553UL;
	public const Oop OOP_CLASS_TerminateProcess = 151809UL;
	public const Oop OOP_CLASS_TransactionBacklog = 152065UL;
	public const Oop OOP_CLASS_Warning = 152321UL;
	public const Oop OOP_CLASS_TestFailure = 152577UL;
	public const Oop OOP_CLASS_SignalBufferFull = 152833UL;
	public const Oop OOP_CLASS_InterSessionSignal = 153089UL;
	public const Oop OOP_CLASS_RubyCextData = 153345UL;
	public const Oop OOP_CLASS_IcuLocale = 153601UL;
	public const Oop OOP_CLASS_IcuCollator = 153857UL;
	public const Oop OOP_CLASS_Utf8 = 154113UL;
	public const Oop OOP_CLASS_Unicode7 = 154369UL;
	public const Oop OOP_CLASS_Unicode16 = 154625UL;
	public const Oop OOP_CLASS_Unicode32 = 154881UL;

	public const Oop OOP_CLASS_ObsoleteGsHostProcess = 155137UL;/* GsHostProcess in Gs64 v3.6.3 and prior*/

	/* added in 3.2 */
	public const Oop OOP_CLASS_BTREE_UNICODE_COMPARISON_QUERY_SPEC = 155393UL;/* v1.1 oop  1215 */
	public const Oop OOP_CLASS_BTREE_UNICODE_RANGE_COMPARISON_QUERY_SPEC = 155649UL;/* v1.1 oop  1217 */

	/* added in 3.2.5 */
	public const Oop OOP_CLASS_UnauthorizedObjectStub = 155905UL;

	/* added in 3.3 */
	public const Oop OOP_CLASS_SmallFraction = 156161UL;
	// 29 bit signed numerator, 27bit unsigned denom, 8 bits tag(value 0x2c), 
	// see lrgint.c and gcisup_ts.c 

	public const Oop OOP_CLASS_AbstractFraction = 156417UL;

	public const Oop OOP_CLASS_LdapDirectoryServer = 156673UL;/* v1.1 oop 1225*/

	public const Oop OOP_AllLdapDirectoryServers = 156929UL;/* oop also coded in image/LdapDirectoryServer.gs*/

	/* added in 3.4 */
	public const Oop OOP_ALL_GROUPS = 157185UL;/* v1.1 oop 1229 */

	public const Oop OOP_CLASS_USER_PROFILE_GROUP = 157441UL;/* v1.1 oop 1231 */

	public const Oop OOP_CLASS_KERBEROS_PRINCIPAL = 157697UL;/* v1.1 oop 1233 */

	public const Oop OOP_ALL_KERBEROS_PRINCIPALS = 157953UL;/* v1.1 oop 1235 */

	/* 46958 */
	public const Oop OOP_CLASS_SecureSocketError = 158209UL;/* v1.1 oop 1237 */

	public const Oop OOP_CLASS_Utf16 = 158465UL;/* v1.1 oop 1239 */

	public const Oop OOP_CLASS_CryptoError = 158721UL;/* v1.1 oop 1241 */

	public const Oop OOP_CLASS_MigrationError = 158977UL;/* v1.1 oop 1243 */

	public const Oop OOP_CLASS_SmallScaledDecimal = 159233UL;/* added in Gs64 v3.6*/
	public const Oop OOP_CLASS_SmallDateAndTime = 159489UL;
	public const Oop OOP_CLASS_SmallTime = 159745UL;
	public const Oop OOP_CLASS_SmallDate = 160001UL;
	public const Oop OOP_CLASS_AlmostOutOfStackError = 160257UL;
	public const Oop OOP_CLASS_AlmostOutOfMemoryError = 160513UL;
	public const Oop OOP_CLASS_GsHostProcess = 160769UL;
	public const Oop OOP_CLASS_CCalloutStructs = 161025UL;

	public const Oop OOP_LAST_KERNEL_OOP = 161025UL;

	/* from 160001, 175 oops left to LAST_EXPORTED_OOP=205057 */

	/* oops OOP_LAST_KERNEL_OOP+256 to OOP_LAST_EXPORTED_OOP(in oop.ht) 
	 * are available for new kernel classes
	 */

	/* ************************************************************ */
	public const Oop OOP_CLASS_old_EXCEPTION = 230913UL;/* v1.1 oop  1805 */
	public const Oop OOP_GEMSTONE_ERROR_CAT = 231169UL;/* v1.1 oop  1807 */
	/* GemStone errors are in this category */

	public const Oop OOP_ALL_CLUSTER_BUCKETS = 232961UL;/* v1.1 oop  1821 */

	public const Oop OOP_EMPTY_INVARIANT_ARRAY = 233217UL;/* v1.1 oop  1823 */
	public const Oop OOP_EMPTY_INVARIANT_STRING = 233473UL;/* v1.1 oop  1825 */
	public const Oop OOP_EMPTY_SYMBOL = 233729UL;/* v1.1 oop  1827 */

	/* oop of kernel symbol #manualBegin , also in oop.ht */
	public const Oop OOP_SYM_MANUAL_BEGIN = 236033UL;/* v1.1 oop 1845 */

	/* oop of kernel symbol #autoBegin , also in oop.ht*/
	public const Oop OOP_SYM_AUTO_BEGIN = 236289UL;/* v1.1 oop 1847 */
}
