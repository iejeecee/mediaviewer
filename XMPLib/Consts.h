#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;


public ref class Consts
{


public:

	static const String ^XMP_NS_XMP = "http://ns.adobe.com/xap/1.0/";

	static const String ^XMP_NS_XMP_Rights = "http://ns.adobe.com/xap/1.0/rights/";
	static const String ^XMP_NS_XMP_MM = "http://ns.adobe.com/xap/1.0/mm/";
	static const String ^XMP_NS_XMP_BJ = "http://ns.adobe.com/xap/1.0/bj/";

	static const String ^XMP_NS_PDF = "http://ns.adobe.com/pdf/1.3/";
	static const String ^XMP_NS_Photoshop = "http://ns.adobe.com/photoshop/1.0/";
	static const String ^XMP_NS_PSAlbum = "http://ns.adobe.com/album/1.0/";
	static const String ^XMP_NS_EXIF = "http://ns.adobe.com/exif/1.0/";
	static const String ^XMP_NS_EXIF_Aux = "http://ns.adobe.com/exif/1.0/aux/";
	static const String ^XMP_NS_TIFF = "http://ns.adobe.com/tiff/1.0/";
	static const String ^XMP_NS_PNG = "http://ns.adobe.com/png/1.0/";
	static const String ^XMP_NS_SWF = "http://ns.adobe.com/swf/1.0/";
	static const String ^XMP_NS_JPEG = "http://ns.adobe.com/jpeg/1.0/";
	static const String ^XMP_NS_JP2K = "http://ns.adobe.com/jp2k/1.0/";
	static const String ^XMP_NS_CameraRaw = "http://ns.adobe.com/camera-raw-settings/1.0/";
	static const String ^XMP_NS_DM = "http://ns.adobe.com/xmp/1.0/DynamicMedia/";
	static const String ^XMP_NS_Script = "http://ns.adobe.com/xmp/1.0/Script/";
	static const String ^XMP_NS_ASF = "http://ns.adobe.com/asf/1.0/";
	static const String ^XMP_NS_WAV = "http://ns.adobe.com/xmp/wav/1.0/";
	static const String ^XMP_NS_BWF = "http://ns.adobe.com/bwf/bext/1.0/";

	static const String ^XMP_NS_XMP_Note = "http://ns.adobe.com/xmp/note/";

	static const String ^XMP_NS_AdobeStoPhoto = "http://ns.adobe.com/StoPhoto/1.0/";
	static const String ^XMP_NS_CreatorAtom = "http://ns.adobe.com/creatorAtom/1.0/";



	static const String ^XMP_NS_XMP_IdentifierQual = "http://ns.adobe.com/xmp/Identifier/qual/1.0/";
	static const String ^XMP_NS_XMP_Dimensions = "http://ns.adobe.com/xap/1.0/sType/Dimensions#";
	static const String ^XMP_NS_XMP_Text = "http://ns.adobe.com/xap/1.0/t/";
	static const String ^XMP_NS_XMP_PagedFile = "http://ns.adobe.com/xap/1.0/t/pg/";
	static const String ^XMP_NS_XMP_Graphics = "http://ns.adobe.com/xap/1.0/g/";
	static const String ^XMP_NS_XMP_Image = "http://ns.adobe.com/xap/1.0/g/img/";
	static const String ^XMP_NS_XMP_Font = "http://ns.adobe.com/xap/1.0/sType/Font#";
	static const String ^XMP_NS_XMP_ResourceEvent = "http://ns.adobe.com/xap/1.0/sType/ResourceEvent#";
	static const String ^XMP_NS_XMP_ResourceRef = "http://ns.adobe.com/xap/1.0/sType/ResourceRef#";
	static const String ^XMP_NS_XMP_ST_Version = "http://ns.adobe.com/xap/1.0/sType/Version#";
	static const String ^XMP_NS_XMP_ST_Job = "http://ns.adobe.com/xap/1.0/sType/Job#";
	static const String ^XMP_NS_XMP_ManifestItem = "http://ns.adobe.com/xap/1.0/sType/ManifestItem#";


	static const String ^XMP_NS_XMP_T = "http://ns.adobe.com/xap/1.0/t/";
	static const String ^XMP_NS_XMP_T_PG = "http://ns.adobe.com/xap/1.0/t/pg/";
	static const String ^XMP_NS_XMP_G_IMG = "http://ns.adobe.com/xap/1.0/g/img/";

	static const String ^XMP_NS_DC = "http://purl.org/dc/elements/1.1/";

	static const String ^XMP_NS_IPTCCore = "http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/";

	static const String ^XMP_NS_DICOM = "http://ns.adobe.com/DICOM/";

	static const String ^XMP_NS_PDFA_Schema = "http://www.aiim.org/pdfa/ns/schema#";
	static const String ^XMP_NS_PDFA_Property = "http://www.aiim.org/pdfa/ns/property#";
	static const String ^XMP_NS_PDFA_Type = "http://www.aiim.org/pdfa/ns/type#";
	static const String ^XMP_NS_PDFA_Field = "http://www.aiim.org/pdfa/ns/field#";
	static const String ^XMP_NS_PDFA_ID = "http://www.aiim.org/pdfa/ns/id/";
	static const String ^XMP_NS_PDFA_Extension = "http://www.aiim.org/pdfa/ns/extension/";

	static const String ^XMP_NS_PDFX = "http://ns.adobe.com/pdfx/1.3/";
	static const String ^XMP_NS_PDFX_ID = "http://www.npes.org/pdfx/ns/id/";

	static const String ^XMP_NS_RDF = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
	static const String ^XMP_NS_XML = "http://www.w3.org/XML/1998/namespace";

	static const int XMP_ArrayLastItem  =  ((XMP_Index)(-1L));

	enum class PropOptions {

		XMP_NoOptions          = ((XMP_OptionBits)0UL),

		/// The XML string form of the property value is a URI, use rdf:resource attribute. DISCOURAGED
		XMP_PropValueIsURI       = 0x00000002UL,

		// ------------------------------------------------------
		// Options relating to qualifiers attached to a property.

		/// The property has qualifiers, includes \c rdf:type and \c xml:lang.
		XMP_PropHasQualifiers    = 0x00000010UL,

		/// This is a qualifier for some other property, includes \c rdf:type and \c xml:lang.
		/// Qualifiers can have arbitrary structure, and can themselves have qualifiers. If the
		/// qualifier itself has a structured value, this flag is only set for the top node of the
		/// qualifier's subtree.
		XMP_PropIsQualifier      = 0x00000020UL,

		/// Implies \c #XMP_PropHasQualifiers, property has \c xml:lang.
		XMP_PropHasLang          = 0x00000040UL,

		/// Implies \c #XMP_PropHasQualifiers, property has \c rdf:type.
		XMP_PropHasType          = 0x00000080UL,

		// --------------------------------------------
		// Options relating to the data structure form.

		/// The value is a structure with nested fields.
		XMP_PropValueIsStruct    = 0x00000100UL,

		/// The value is an array (RDF alt/bag/seq). The "ArrayIs..." flags identify specific types
		/// of array; default is a general unordered array, serialized using an \c rdf:Bag container.
		XMP_PropValueIsArray     = 0x00000200UL,

		/// The item order does not matter.
		XMP_PropArrayIsUnordered = XMP_PropValueIsArray,

		/// Implies \c #XMP_PropValueIsArray, item order matters. It is serialized using an \c rdf:Seq container.
		XMP_PropArrayIsOrdered   = 0x00000400UL,

		/// Implies \c #XMP_PropArrayIsOrdered, items are alternates. It is serialized using an \c rdf:Alt container.
		XMP_PropArrayIsAlternate = 0x00000800UL,

		// ------------------------------------
		// Additional struct and array options.

		/// Implies \c #XMP_PropArrayIsAlternate, items are localized text.  Each array element is a
		/// simple property with an \c xml:lang attribute.
		XMP_PropArrayIsAltText   = 0x00001000UL,

		// XMP_InsertBeforeItem  = 0x00004000UL,  ! Used by SetXyz functions.
		// XMP_InsertAfterItem   = 0x00008000UL,  ! Used by SetXyz functions.

		// ----------------------------
		// Other miscellaneous options.

		/// This property is an alias name for another property.  This is only returned by
		/// \c TXMPMeta::GetProperty() and then only if the property name is simple, not an path expression.
		XMP_PropIsAlias          = 0x00010000UL,

		/// This property is the base value (actual) for a set of aliases.This is only returned by
		/// \c TXMPMeta::GetProperty() and then only if the property name is simple, not an path expression.
		XMP_PropHasAliases       = 0x00020000UL,

		/// The value of this property is "owned" by the application, and should not generally be editable in a UI.
		XMP_PropIsInternal       = 0x00040000UL,

		/// The value of this property is not derived from the document content.
		XMP_PropIsStable         = 0x00100000UL,

		/// The value of this property is derived from the document content.
		XMP_PropIsDerived        = 0x00200000UL,

		// XMPUtil_AllowCommas   = 0x10000000UL,  ! Used by TXMPUtils::CatenateArrayItems and ::SeparateArrayItems.
		// XMP_DeleteExisting    = 0x20000000UL,  ! Used by TXMPMeta::SetXyz functions to delete any pre-existing property.
		// XMP_SchemaNode        = 0x80000000UL,  ! Returned by iterators - #define to avoid warnings

		// ------------------------------
		// Masks that are multiple flags.

		/// Property type bit-flag mask for all array types
		XMP_PropArrayFormMask    = XMP_PropValueIsArray | XMP_PropArrayIsOrdered | XMP_PropArrayIsAlternate | XMP_PropArrayIsAltText,

		/// Property type bit-flag mask for composite types (array and struct)
		XMP_PropCompositeMask    = XMP_PropValueIsStruct | XMP_PropArrayFormMask,

		/// Mask for bits that are reserved for transient use by the implementation.
		XMP_ImplReservedMask     = 0x70000000L,

		/// Option for array item location: Insert a new item before the given index.
		XMP_InsertBeforeItem      = 0x00004000UL,

		/// Option for array item location: Insert a new item after the given index.
		XMP_InsertAfterItem       = 0x00008000UL,

		/// Delete any pre-existing property.
		XMP_DeleteExisting        = 0x20000000UL,

		/// Bit-flag mask for property-value option bits
		XMP_PropValueOptionsMask  = XMP_PropValueIsURI,

		/// Bit-flag mask for array-item location bits
		XMP_PropArrayLocationMask = XMP_InsertBeforeItem | XMP_InsertAfterItem

	};

	enum class OpenOptions {

		/// Open for read-only access.
		XMPFiles_OpenForRead           = 0x00000001,

		/// Open for reading and writing.
		XMPFiles_OpenForUpdate         = 0x00000002,

		/// Only the XMP is wanted, allows space/time optimizations.
		XMPFiles_OpenOnlyXMP           = 0x00000004,

		/// Be strict about only attempting to use the designated file handler, no fallback to other handlers.
		XMPFiles_OpenStrictly          = 0x00000010,

		/// Require the use of a smart handler.
		XMPFiles_OpenUseSmartHandler   = 0x00000020,

		/// Force packet scanning, do not use a smart handler.
		XMPFiles_OpenUsePacketScanning = 0x00000040,

		/// Only packet scan files "known" to need scanning.
		XMPFiles_OpenLimitedScanning   = 0x00000080,

		/// Attempt to repair a file opened for update, default is to not open (throw an exception).
		XMPFiles_OpenRepairFile        = 0x00000100

	};

	enum class IterOptions {

		/// The low 8 bits are an enum of what data structure to iterate.
		XMP_IterClassMask      = 0x00FFUL,

		/// Iterate the property tree of a TXMPMeta object.
		XMP_IterProperties     = 0x0000UL,

		/// Iterate the global alias table.
		XMP_IterAliases        = 0x0001UL,

		/// Iterate the global namespace table.
		XMP_IterNamespaces     = 0x0002UL,

		/// Just do the immediate children of the root, default is subtree.
		XMP_IterJustChildren   = 0x0100UL,

		/// Just do the leaf nodes, default is all nodes in the subtree.
		XMP_IterJustLeafNodes  = 0x0200UL,

		/// Return just the leaf part of the path, default is the full path.
		XMP_IterJustLeafName   = 0x0400UL,

		/// Omit all qualifiers.
		XMP_IterOmitQualifiers = 0x1000UL

	};
};
