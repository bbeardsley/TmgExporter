# The Master Genealogist (TMG) File Structures

for v9

Last Updated: July 2014

**COPYRIGHT © 2014, Wholly Genes, Inc. All Rights Reserved.**

## Table of Contents

General Discussion

The file structures in TMG are described below as an aid to developers
and those seeking direct access to TMG's data files. We encourage
third-party development because it provides further options to our
users.

While the basic design and fundamental file associations are expected to
remain stable for the foreseeable future, Wholly Genes, Inc., reserves
the right to change TMG's file structures at any time without notice.
Serious developers should write to the Technical Support Department for
complete and up-to-date file descriptions.

Wholly Genes, Inc., further reserves the right to develop, license,
market, and sell feature enhancements and/or external utilities which
operate on TMG's data files and which might compete or conflict with
those provided by third-party vendors. In an effort to foster a
cooperative spirit among developers and provide the best possible
service to our customers, developers are encouraged to inquire about
work in progress before proceeding.

**Warning** We strongly discourage access to TMG's data files from
outside the controlled parameters of the program environment by users
who are not skilled in relational database design and xBase programming
techniques. In all circumstances, Wholly Genes, Inc., specifically
disavows any responsibility for damage or loss of these data files which
might result from access by other parties. In particular, the accuracy
of the file structures in this document is not guaranteed, nor are these
file structures certified for any particular purpose. Back up often ---
and proceed at your own risk!

*GenBridge*

Rather than access TMG's data files directly, developers are encouraged
whenever possible to use the GenBridge Software Development Kit (SDK)
that has been provided for that purpose (see ) . The GenBridge API
allows the developer to access TMG's principal data fields through a
series of simple function calls and does not require the use of any
xBase-compatible development tools. The use of GenBridge for this
purpose falls within the freeware license.

Among other advantages of the GenBridge SDK to the software developer,
the same code base can optionally be used to access data that is stored
in **other program formats** (including Personal Ancestral File™, Family
Tree Maker™, and others) with minimal code changes. Use of GenBridge for
those purpose *may* require a commercial license. See the GenBridge
documentation for more information.

This document may proof useful to GenBridge developers insofar as
understanding the TMG data types and the relationships between them.

*Data File Compatibility*

Information which is entered in TMG is stored in a series of
Foxpro-compatible database files. While many of these files may be read
or manipulated by database managers such as dBase®, CodeBase®, and
Paradox®, complete compatibility with such systems should not be
assumed. In particular, while TMG's database (DBF) files are technically
compatible with those produced by dBase III and dBase IV, the memo (FPT)
files which are associated with some of those databases are **not**.
Attempts to access those database files through dBase or other utility
may render them incompatible or unreadable by Foxpro and TMG.

Structural compound index (CDX) files are likewise Foxpro-compatible.
However, because the user-defined functions which are utilized as index
keys are compiled into the program, they are not accessible to
third-party developers. Attempts to open these data files from Foxpro
will generate a "File does not exist" or similar error.

TMG's database (DBF) files are encoded with the code page which was in
use when it was created (or updated by a patch). The default code page
is 1252 (U.S. Windows). All DBF files within a single TMG project are
expected to be marked with the same code page. The collation sequence
may be any that is supported by Visual Foxpro but is also expected to be
the same throughout any indexes for a single project.

*Modifying files*

The data in TMG's database files may be manipulated by careful
developers according to the specific guidelines in this document.

Access to TMG's data file should be made in shared (not exclusive) mode
if there is any chance that they will be accessed simultaneously by TMG
itself or another utility. It is also advisable to lock records to which
you are writing so that TMG (or other utility) does not attempt to
modify the same record at the same time. Take care when manually
deleting records that linked records are likewise deleted when
appropriate.

For all but a few global maintenance functions, TMG accesses its own
data files in shared mode. It employs file- and record-locking as
appropriate.

With the notable exception of the Person file (\$.dbf) described later,
the experienced developer may rearrange or add additional fields to any
TMG data file to suit a particular purpose. You should **NOT** delete
default fields, however, nor change their names, types, lengths, or
other attributes. Doing so may cause abnormal terminations of TMG and
may risk **unrecoverable corruption of the data files**.

Developers who wish to create their own index files or keys are
encouraged to use standard index (IDX) files, rather than structural
compound index (CDX) files, in order to avoid conflicts with index keys
created by TMG. Among other concerns, the speed of TMG may be severely
degraded by the presence of unused or unrecognized index keys.

*TMG Versions*

This document describes the file structures of TMG v5 through TMG v9,
with noted distinctions between those versions when appropriate.

Versions prior to v5 are described in a separate document which is
available from TMGFSTR.ZIP available from

*Basic Organization*

All files associated with a single TMG project are contained in a single
folder which may be on any accessible drive and path.

All file names for the project begin with the project name followed by
an underscore. For instance, all files in the SAMPLE project (i.e.,
C:\\TMG\\SAMPLE folder) begin with "SAMPLE\_". The subsequent characters
and file extension identify the contents of that file.

\
A TMG project includes files with the following extensions:

DBF Foxpro database files

FPT Foxpro memo files (supports memo fields, when present)

CDX Foxpro structural compound indexes

****PJC Project configuration and version control file (ASCII text)

DOC Documentation file (ASCII text)

Each database file type is described in detail in the following
sections.

Other file extensions (ACC, FLP, etc.) may also be present in the
project folder and used for configuration or other administrative
purposes. No attempt is made in this document to describe the structures
of such files as they are not strictly part of the project and do not
contain genealogical data. For assistance in accessing any files not
specifically described in this document, please contact Wholly Genes,
Inc., Technical Support Department.

Record Layouts

This section describes the file structure or record layout for the
database files within a TMG project. A small amount of redundancy is
intentionally included to enhance performance on the Picklist and other
operations.

For each table, the following information is provided:

- Field Name

- Field Type (C=character, I=integer, N=numeric, L=logical, M=memo)

- Field Width

- Description

For most tables, a "primary key" field is identified. This field can be
used to uniquely reference a record within that table. Tables without a
primary key are associative in nature and facilitate many-to-many
relationships between other tables. For instance, the Focus Group Member
file (B.DBF) links people to focus groups.

When appropriate, the field description also includes references to
linked fields. For instance "n.recno" refers to the "recno" field within
the "n" data file (e.g., sample_n.dbf). Unless otherwise noted, such
links are many-to-one and refer to a primary key field.

Total record length includes one byte for the DELETED() flag, as is
customary in xBase languages. By default, the memo block size is set to
64 bytes and the code page is set to 1252 (or the user's current code
page).

*Person File (\$.DBF)*

The record length of this file will vary with the addition of
user-defined flags.

**Warning** Do not change, add, or reorder fields in this file.

+----------------+----------+-----------+------------------------------+
| **Field Name** | **Type** | **Width** | **Description**              |
+----------------+----------+-----------+------------------------------+
| PER_NO         | I        | 4         | . of the person.             |
+----------------+----------+-----------+------------------------------+
| FATHER         | I        | 4         | of the primary father        |
+----------------+----------+-----------+------------------------------+
| MOTHER         | I        | 4         | of the primary mother        |
+----------------+----------+-----------+------------------------------+
| LAST_EDIT      | D        | 8         | Date this person was last    |
|                |          |           | edited                       |
+----------------+----------+-----------+------------------------------+
| DSID           | I        | 4         |                              |
+----------------+----------+-----------+------------------------------+
| REF_ID         | I        | 4         |                              |
+----------------+----------+-----------+------------------------------+
| REFERENCE      | C        | 12\*      | User-defined Reference       |
|                |          |           | value.                       |
|                |          |           |                              |
|                |          |           | \*The length of this field   |
|                |          |           | may be customized by the     |
|                |          |           | user.                        |
+----------------+----------+-----------+------------------------------+
| SPOULAST       | I        | 4         | of last spouse with whom     |
|                |          |           | this person was paired on a  |
|                |          |           | Family or Tree View.         |
+----------------+----------+-----------+------------------------------+
| SCBUFF         | C        | 10        | Administrative field -- no   |
|                |          |           | data                         |
+----------------+----------+-----------+------------------------------+
| PBIRTH         | C        | 30        | Primary birth                |
+----------------+----------+-----------+------------------------------+
| PDEATH         | C        | 30        | Primary death                |
+----------------+----------+-----------+------------------------------+
| SEX            | C        | 1         | Sex flag (?/M/F)             |
+----------------+----------+-----------+------------------------------+
| LIVING         | C        | 1         | Living flag (?/Y/N)          |
+----------------+----------+-----------+------------------------------+
| BIRTHORDER     | C        | 2         | Sequential birth order among |
|                |          |           | full siblings                |
+----------------+----------+-----------+------------------------------+
| MULTIBIRTH     | C        | 1         | Multiple Birth flag (?/Y/N)  |
+----------------+----------+-----------+------------------------------+
| ADOPTED        | C        | 1         | Adopted flag (?/Y/N)         |
+----------------+----------+-----------+------------------------------+
| ANCE_INT       | C        | 1         | Ancestor Interest flag (0-3) |
+----------------+----------+-----------+------------------------------+
| DESC_INT       | C        | 1         | Descendant Interest flag     |
|                |          |           | (0-3)                        |
+----------------+----------+-----------+------------------------------+
| RELATE         | I        | 4         | Relationship calculation     |
+----------------+----------+-----------+------------------------------+
| RELATEFO       | I        | 4         | Relationship calculation     |
+----------------+----------+-----------+------------------------------+
| TT             | C        | 1         | Administrative field -- no   |
|                |          |           | data                         |
+----------------+----------+-----------+------------------------------+
| FLAG1          | C        | 1         | User-defined flag as defined |
|                |          |           | in the Flag File.            |
|                |          |           |                              |
|                |          |           | Relates to c.flagfield       |
+----------------+----------+-----------+------------------------------+
| FLAG2, etc.    | C        | 1         | As above.                    |
+----------------+----------+-----------+------------------------------+

*Source Type File (A.DBF)*

  ---------------- ---------- ----------- ---------------------------------------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  RULESET          N          1           1=Mills Categories, 2=Lackey Categories, 3=Custom Categories
  DSID             I          4           
  SOURTYPE         I          4           Type within this ruleset.
  TRANS_TO         I          4           Most like which source type within Mills' Categories?
  NAME             C          66          Name of this source type
  FOOT             M          4           Standard full footnote template
  SHORT            M          4           Standard short footnote template
  BIB              M          4           Standard bibliographic template
  CUSTFOOT         M          4           Custom full footnote template
  CUSTSHORT        M          4           Custom short footnote template
  CUSTBIB          M          4           Custom bibliographic template
  SAMEAS           I          4           This source type is the same as another in the Mills Categories. Relates to a.sourtype where ruleset=1.
  SAMEASMSG        M          4           The message displayed to the user when this source type is selected.
  PRIMARY          L          1           Is this the primary custom source type (ruleset=3).
  REMINDERS        M          4           Hint Memo (new in v7.01)
  TT               C          1           Administrative field -- no data
  ---------------- ---------- ----------- ---------------------------------------------------------------------------------------------------------

*Focus Group Member File (B.DBF)*

  ---------------- ---------- ----------- ------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  GROUPNUM         I          4           of the Focus Group
  MEMBERNUM        I          4           of the person (Focus Group member)
  TT               C          1           Administrative field -- no data
  DSID             I          4           
  ---------------- ---------- ----------- ------------------------------------

*Flag File (C.DBF)*

+----------------+----------+-----------+------------------------------+
| **Field Name** | **Type** | **Width** | **Description**              |
+----------------+----------+-----------+------------------------------+
| FLAGLABEL      | C        | 50        | Flag label                   |
+----------------+----------+-----------+------------------------------+
| FLAGFIELD      | C        | 10        | Corresponding field name in  |
|                |          |           | \$.dbf for flag values       |
+----------------+----------+-----------+------------------------------+
| FLAGVALUE      | C        | 250       | Possible flag values         |
+----------------+----------+-----------+------------------------------+
| SEQUENCE       | I        | 4         | Order of flags in the flag   |
|                |          |           | window                       |
+----------------+----------+-----------+------------------------------+
| DESCRIPT       | M        | 4         | Flag description             |
+----------------+----------+-----------+------------------------------+
| ACTIVE         | L        | 1         | Is this flag visible?        |
+----------------+----------+-----------+------------------------------+
| FLAGID         | I        | 4         | . Flag                       |
+----------------+----------+-----------+------------------------------+
| PROPERTY       | M        | 4         | Additional information for   |
|                |          |           | flags                        |
|                |          |           |                              |
|                |          |           | (translations to other       |
|                |          |           | languages) (new in v8.0)     |
+----------------+----------+-----------+------------------------------+
| DSID           | I        | 4         |                              |
+----------------+----------+-----------+------------------------------+
| TT             | C        | 1         | Administrative field -- no   |
|                |          |           | data                         |
+----------------+----------+-----------+------------------------------+

*Data Set File (D.DBF)*

  ---------------- ---------- ----------- ---------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  DSID             I          4           .
  DSNAME           C          100         Data set name
  DSLOCATION       C          253         Location from which the data set was originally imported
  DSTYPE           N          2           Type of data set from which it was originally imported
  DSLOCKED         L          1           Is this data set locked?
  DSENABLED        L          1           Is this data set enabled?
  PROPERTY         M          4           Import wizard properties
  DSP              M          4           Import wizard properties (tag mapping)
  DSP2             M          4           Import wizard properties (new in v8.0)
  TT               C          1           Administrative field -- no data
  DCOMMENT         M          4           Data set comment
  HOST             M          4           Import wizard properties (tag renaming)
  NAMESTYLE        I          4           Default name style for this data set. Relates to st.styleid.
  PLACESTYLE       I          4           Default place style for this data set. Relates to st.styleid.
  ---------------- ---------- ----------- ---------------------------------------------------------------

*DNA File (DNA.DBF)*

This entire table is new in v6

+----------------+----------+-----------+------------------------------+
| **Field Name** | **Type** | **Width** | **Description**              |
+----------------+----------+-----------+------------------------------+
| DSID           | I        | 4         |                              |
+----------------+----------+-----------+------------------------------+
| ID_DNA         | I        | 4         | . DNA fingerprint .          |
+----------------+----------+-----------+------------------------------+
| ID_PERSON      | I        | 4         | of the linked person.        |
+----------------+----------+-----------+------------------------------+
| DNANAME        | C        | 100       | DNA fingerprint test name    |
+----------------+----------+-----------+------------------------------+
| COMMENTS       | M        | 4         | User comments                |
+----------------+----------+-----------+------------------------------+
| DESCRIPT       | M        | 4         | DNA fingerprint test         |
|                |          |           | description                  |
+----------------+----------+-----------+------------------------------+
| RESULT         | M        | 4         | DNA fingerprint test results |
+----------------+----------+-----------+------------------------------+
| URL            | M        | 4         | DNA laboratory URL           |
+----------------+----------+-----------+------------------------------+
| LOGO           | M        | 4         | DNA laboratory logo          |
+----------------+----------+-----------+------------------------------+
| TT             | C        | 1         | Administrative field -- no   |
|                |          |           | data                         |
+----------------+----------+-----------+------------------------------+
| DESCRIPT       | M        | 4         | Description (new in v6.04)   |
+----------------+----------+-----------+------------------------------+
| KITNUMBER      | C        | 24        | Kit number for this test     |
|                |          |           | (new in v7.04)               |
+----------------+----------+-----------+------------------------------+
| TYPE           | C        | 1         | Type of data (new in v7.04)  |
|                |          |           |                              |
|                |          |           | A-actual test result         |
|                |          |           |                              |
|                |          |           | D-derived signatures         |
|                |          |           |                              |
|                |          |           | M-modal signatures           |
+----------------+----------+-----------+------------------------------+
| NAMEREC        | I        | 4         | Number record (new in v7.04) |
+----------------+----------+-----------+------------------------------+

*Event Witness File (E.DBF)*

Note: Principals are also represented in this file because principals
are also witnesses.

  ---------------- ---------- ----------- -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  EPER             I          4           of the witness
  GNUM             I          4           Event Number. Relates to g.recno
  PRIMARY          L          1           Is the event primary for this person? Must be FALSE if witness is not a principal.
  WSENTENCE        M          4           Witness sentence structure. Local sentence for this person overrides t.sentence. The field may contain one sentence structure for each supported language. Each sentence structure is preceded by "\[L=LanguageName\]".
  TT               C          1           Administrative field -- no data
  ROLE             C          5           The role of this person in the event.
  DSID             I          4           
  NAMEREC          I          4           The name used by this witness in the event. Relates to n.recno.
  WITMEMO          M          4           Witness memo
  SEQUENCE         I          4           Sequence of witnesses for this event (new in v6.0)
  ---------------- ---------- ----------- -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

*Parent/Child Relationship File (F.DBF and F.FPT)*

  ---------------- ---------- ----------- -------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  PRIMARY          L          1           Is it a primary relationship?
  CHILD            I          4           of the child
  PARENT           I          4           of the parent
  PTYPE            I          4           Relationship type. Relates to t.etypenum.
  PNOTE            M          4           Relationship memo
  PSURE            C          1           Max of the relationship
  FSURE            C          1           Max of the relationship memo
  RECNO            I          4           . Relationship record ID.
  TT               C          1           Administrative field -- no data
  DSID             I          4           
  ---------------- ---------- ----------- -------------------------------------------

*Event File (G.DBF and G.FPT)*

  ---------------- ---------- ----------- -------------------------------------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  ETYPE            I          4           Event type. Relates to t.etypenum.
  DSID             I          4           
  PER1SHOW         L          1           Display Principal 1 on the Person View?
  PER2SHOW         L          1           Display Principal 2 on the Person View?
  PER1             I          4           of Principal 1
  PER2             I          4           of Principal 2
  EDATE            C          30          Event
  PLACENUM         I          4           Event place. Relates to p.recno.
  EFOOT            M          4           Event memo
  ENSURE           C          1           Max of Principal 1 (i.e., the maximum surety value in this category for all citations to this event).
  ESSURE           C          1           Max of Principal 2
  EDSURE           C          1           Max of the event date
  EPSURE           C          1           Max of the event place
  EFSURE           C          1           Max of the event memo
  RECNO            I          4           . Event record number
  SENTENCE         M          4           Sentence Construction Rules
  SRTDATE          C          30          Event sort
  TT               C          1           Administrative field -- no data
  REF_ID           I          4           as preserved from an import.
  ---------------- ---------- ----------- -------------------------------------------------------------------------------------------------------

*Exhibit File (I.DBF and I.FPT)*

  ---------------- ---------- ----------- --------------------------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  IDEXHIBIT        I          4           . Exhibit number.
  IDREF            I          4           
  RLTYPE           C          1           Exhibit type. For backward compatibility. Use ID_Person, ID_Event, etc., instead.
  RLNUM            I          4           Exhibit linked record. For backward compatibility. Use ID_Person, ID_Event, etc., instead.
  XNAME            C          30          Exhibit name
  VFILENAME        M          4           Video exhibit external file name
  IFILENAME        M          4           Image exhibit external file name
  AFILENAME        M          4           Audio exhibit external file name
  TFILENAME        M          4           Text exhibit external file name
  REFERENCE        C          25          Exhibit reference value.
  TEXT             M          4           Text internal exhibit
  IMAGE            M          4           Image internal exhibit
  AUDIO            M          4           Audio internal exhibit
  DESCRIPT         M          4           Exhibit description.
  RLPER1           I          4           of Principal 1 in the linked event.
  RLPER2           I          4           of Principal 2 in the linked event.
  RLGTYPE          I          4           Event type of the linked event. Related to t.etypenum.
  OLE_OBJECT       G          4           OLE Exhibit
  PRIMARY          L          1           Is this the primary exhibit for this person? (Person exhibits only)
  VIDEO            M          4           Video internal exhibit
  PROPERTY         M          4           Exhibit properties.
  DSID             I          4           
  TT               C          1           Administrative field -- no data
  ID_PERSON        I          4           of the linked person. Related to \$.per_no.
  ID_EVENT         I          4           of the linked event. Related to g.recno.
  ID_SOURCE        I          4           of the linked source. Related to m.majnum.
  ID_REPOS         I          4           of the linked repository. Related to r.recno.
  THUMB            M          4           Thumbnail of the image exhibit
  ID_CIT           I          4           of the linked citation. Related to s.recno.
  ID_PLACE         I          4           of the linked place. Related to p.recno.
  CAPTION          M          4           Image caption (new in v6.0)
  SORTEXH          I          4           Exhibit output order (new in v6.01)
  IMAGEFORE        M          4           Fore Image for exhibit (new in v7.0)
  IMAGEBACK        M          4           Back Image for exhibit (new in v7.0)
  TRANSPAR         N          4           Transparency value (new in v7.0)
  ---------------- ---------- ----------- --------------------------------------------------------------------------------------------

*Timeline Lock File (K.DBF)*

  ---------------- ---------- ----------- ---------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  IDLOCK           I          4           of the person
  TNAME            C          50          Timeline file name
  DSID             I          4           
  TT               C          1           Administrative field -- no data
  ---------------- ---------- ----------- ---------------------------------

*Research Log File (L.DBF and L.FPT)*

  ---------------- ---------- ----------- ------------------------------------------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  RLTYPE           C          1           Research task link type (\_=person, M=source, E=event, R=repository).
  RLNUM            I          4           Linked record number. Related according to RLTYPE (\$.per_no, m.majnum, g.recno, or r.recno, respectively)
  RLPER1           I          4           of Principal 1 in the linked event.
  RLPER2           I          4           of Principal 2 in the linked event.
  RLGTYPE          I          4           Tag type of the linked event (related to t.etypenum)
  TASK             M          4           Task memo
  RLEDITED         C          11          Last edited
  DESIGNED         C          11          Designed (exact date required)
  BEGUN            C          11          Begin (exact date required)
  PROGRESS         C          11          Progress (exact date required)
  COMPLETED        C          11          Completed (exact date required).
  PLANNED          C          11          Planned (exact date required)
  EXPENSES         N          14,2        Task expenses
  COMMENTS         M          4           Text for task steps (Designed\$!&Planned\$!&Begun\$!&Progress\$!&Completed)
  RLNOTE           M          4           Task comments
  KEYWORDS         M          4           Task keywords
  DSID             I          4           
  ID_PERSON        I          4           of the linked person. Related to \$.per_no.
  ID_EVENT         I          4           of the linked event. Related to g.recno.
  ID_SOURCE        I          4           of the linked source. Related to m.majnum.
  ID_REPOS         I          4           of the linked repository. Related to r.recno.
  TT               C          1           Administrative field -- no data
  REFERENCE        C          30          Reference value. Used internally only (e.g. to track links to GenSmarts). (new in v6.0)
  ---------------- ---------- ----------- ------------------------------------------------------------------------------------------------------------

*Source File (M.DBF)*

  ---------------- ---------- ----------- ----------------------------------------------------------------------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  MACTIVE          L          1           Is this source displayed on the default Master Source List?
  MAJNUM           I          4           . of the source
  REF_ID           I          4           
  ABBREV           C          50          Source abbreviation
  DEFSURE          C          1           Default for this source
  TITLE            M          4           Source full title
  TYPE             N          3           Source Type. Relates to a.sourcetype where a.ruleset=1.
  RECORDER         N          2           Source Recorder Type (1=Other, 2=Person, 3=Civil, 4=Church, 5=Commercial, 6=Institutional)
  MEDIA            N          2           Media Type (1=Other, 2=Film, 3=Book, 4=Visual, 5=Audio, 6=Memory)
  FIDELITY         N          2           Source Fidelity (1=Other, 2=Original, 3=Photocopy, 4=Transcript, 5=Extract)
  INDEXED          N          1           Indexed (1=Unknown, 2=No, 3=Yes)\\
  STATUS           N          2           Administrative field -- no data
  TEXT             M          4           Source memo
  SPERNO           I          4           of the author
  ISPICKED         L          1           Administrative field -- no data
  INFO             M          4           Values for source elements (data entry fields) for this source, separated by "\$!&". The sequence of fields corresponds to u.groupnum.
  FFORM            M          4           Full footnote template (if present, overrides the one associated with the source type).
  SFORM            M          4           Short footnote template (if present, overrides the one associated with the source type).
  BFORM            M          4           Bibliographic template (if present, overrides the one associated with the source type).
  CITED            L          1           Administrative field -- no data
  IBIDTYPE         N          1           Use "Ibid" for this source? (0=same source, 1=same source and CD, 2=off)
  SUBJECTID        I          4           of the subject
  COMPILERID       I          4           of the compiler
  EDITORID         I          4           of the editor
  SPERNO2          I          4           of the second person
  UNCITEDFLD       M          4           Source Elements that are linked to this source but are not included in any output template.
  CUSTTYPE         I          4           Custom source type. Relates to a.sourcetype where a.ruleset=3.
  FIRSTCD          M          4           Administrative field -- no data
  DSID             I          4           
  REMINDERS        M          4           Hint Memo (new in v7.01)
  TT               C          1           Administrative field -- no data
  ---------------- ---------- ----------- ----------------------------------------------------------------------------------------------------------------------------------------

*Name File (N.DBF and N.FPT)*

  ---------------- ---------- ----------- -----------------------------------------------------------------------------------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  NPER             I          4           of the person
  ALTYPE           I          4           Name tag type. Relates to t.etypenum.
  ISPICKED         L          1           Administrative field -- no data
  INFS             L          1           Flag to indicate whether the surname in this record is inferred from the person's primary name record, rather than entered directly by the user.
  INFG             L          1           Flag to indicate whether the given name in this record is inferred from the person's primary name record, rather than entered directly by the user.
  PRIMARY          L          1           Primary Name for this person?
  NSURE            C          1           Max Name
  FSURE            C          1           Max Memo
  NNOTE            M          4           Name memo
  RECNO            I          4           . Sequential Name Record Number
  SENTENCE         M          4           Sentence structure for this name
  NDATE            C          30          Name
  SRTDATE          C          30          Name sort
  DSURE            C          1           Name date
  DSID             I          4           
  TT               C          1           Administrative field -- no data
  SRNAMESORT       C          70          Name text used for sorting by surname
  GVNAMESORT       C          70          Name text used for sorting by given name
  STYLEID          I          4           of the style. Relates to st.styleid.
  SURID            I          4           Surname Dictionary . Relates to nd.uid.
  GIVID            I          4           Given Name Dictionary . Relates to nd.uid.
  SRNAMEDISP       C          70          Name text used for display when sorted by surname (e.g., on the Picklist).
  SNDXSURN         C          4           Soundex for surname (new in v7.01)
  SNDXGVN          C          4           Soundex for given name (new in v7.01)
  PBIRTH           C          30          Primary birth date (new in v7.01)
  PDEATH           C          30          Primary death date (new in v7.01)
  REFER            C          30          Reference (for index expression) (new in v7.01)
  PREF_ID          I          4           Reference ID of person for current data set (new in v7.01)
  LAST_EDIT        D          8           Date this person was last edited (new in v7.01)
  ---------------- ---------- ----------- -----------------------------------------------------------------------------------------------------------------------------------------------------

*Name Dictionary File (ND.DBF)*

The unique values of the name fields.

  ---------------- ---------- ----------- ---------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  UID              I          4           within the name dictionary.
  VALUE            M          4           Name value
  SDX              C          4           Soundex of name value
  TT               C          1           Administrative field -- no data
  ---------------- ---------- ----------- ---------------------------------

*Name Part Type File (NPT.DBF)*

The possible parts that can comprise a name.

  ---------------- ---------- ----------- -------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  ID               I          4           of the Name Part
  VALUE            C          100         Name Part Type value
  SYSTEM           L          1           Is this a default name part type?
  TYPE             I          4           Name Part Type classification (corresponds to data entry field number).
  SHORTVALUE       C          20          Short version of the Name Part Type
  TT               C          1           Administrative field -- no data
  DSID             I          4           
  TEMPLATE         C          30          Template name part
  ---------------- ---------- ----------- -------------------------------------------------------------------------

*Name Part Value File (NPV.DBF)*

Determines which parts make up each name.

  ---------------- ---------- ----------- -------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  RECNO            I          4           of the Name Record. Relates to n.recno.
  UID              I          4           Name Dictionary . Relates to nd.uid.
  TYPE             I          4           Name Part Type classification (corresponds to data entry field number).
  ID               I          4           The type of this name part. Relates to npt.id.
  TT               C          1           Administrative field -- no data
  DSID             I          4           
  ---------------- ---------- ----------- -------------------------------------------------------------------------

*Focus Group File (O.DBF)*

  ---------------- ---------- ----------- -----------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  GROUPNUM         I          4           . of the Focus Group
  GROUPNAME        C          40          Name of the Focus Group
  RECENT           L          1           Is this the most recently used Focus Group? (new in v6.0)
  TT               C          1           Administrative field -- no data
  ---------------- ---------- ----------- -----------------------------------------------------------

*Place File (P.DBF)*

  ---------------- ---------- ----------- ----------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  RECNO            I          4           PRIMARY KEY. of the Place Record.
  STYLEID          I          4           of the Place Style. Relates to st.styleid.
  DSID             I          4           
  TT               C          1           Administrative field -- no data
  STARTYEAR        C          4           The earliest year in which this place name existed
  ENDYEAR          C          4           The latest year in which this place name existed
  COMMENT          M          4           User comment about this place
  SHORTPLACE       M          4           Short (abbreviated) name of this place
  ---------------- ---------- ----------- ----------------------------------------------------

*Place Dictionary File (PD.DBF)*

The unique values of the place fields.

  ---------------- ---------- ----------- ---------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  UID              I          4           within the Place Dictionary.
  VALUE            M          4           Place value
  SDX              C          4           Soundex of this place value
  TT               C          1           Administrative field -- no data
  ---------------- ---------- ----------- ---------------------------------

*Place Part Type File (PPT.DBF)*

The possible parts that can comprise a place.

  ---------------- ---------- ----------- --------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  ID               I          4           of the Place Part.
  TYPE             I          4           Place Part Type classification (corresponds to data entry field number).
  VALUE            C          100         Place Part Type value
  SYSTEM           L          1           Is this a default place part type?
  SHORTVALUE       C          20          Short version of the Place Part Type
  TT               C          1           Administrative field -- no data
  DSID             I          4           
  ---------------- ---------- ----------- --------------------------------------------------------------------------

*Place Part Value File (PPV.DBF)*

Determines which parts make up each place.

  ---------------- ---------- ----------- --------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  RECNO            I          4           of the Place Record. Relates to p.recno.
  UID              I          4           Place Dictionary . Relates to pd.uid.
  TYPE             I          4           Place Part Type classification (corresponds to data entry field number).
  ID               I          4           The type of this place part. Relates to ppt.id.
  TT               C          1           Administrative field -- no data
  DSID             I          4           
  ---------------- ---------- ----------- --------------------------------------------------------------------------

*Repository File (R.DBF and R.FPT)*

  ---------------- ---------- ----------- -----------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  DSID             I          4           
  NAME             M          4           Repository name
  RECNO            I          4           . Repository record ID.
  REF_ID           I          4           
  ABBREV           C          50          Repository abbreviation
  ADDRESS          I          4           Repository address/place. Related to
  RNOTE            M          4           Repository memo
  RPERNO           I          4           of the Repository Person (if the repository is a person).
  ISPICKED         L          1           Administrative field -- no data
  TT               C          1           Administrative field -- no data
  ---------------- ---------- ----------- -----------------------------------------------------------

*Source Citation File (S.DBF and S.FPT)*

  ---------------- ---------- ----------- ----------------------------------------------------------------------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  RECNO            I          4           . Citation record ID.
  STYPE            C          1           Citation record type (N=name, F=relationship, M=source, E=event, P=Place, C=Citation).
  REFREC           I          4           Cited record ID. Related table is defined by STYPE. Relates to n.recno, f.recno, m.majnum, g.recno, p.recno, or s.recno, respectively.
  MAJSOURCE        I          4           Source ID. Relates to m.majnum.
  SUBSOURCE        M          4           Citation Detail
  SNSURE           C          1           of Principal 1, name, or relationship
  SSSURE           C          1           of Principal 2
  SDSURE           C          1           of date
  SPSURE           C          1           of place
  SFSURE           C          1           of memo
  ISPICKED         L          1           Administrative field -- no data
  SEQUENCE         I          4           The sequential position of this citation among others linked to the same record. (new in v6.0)
  CITMEMO          M          4           Citation Memo (new in v6.0)
  EXCLUDE          L          1           Is it an excluded citation?
  TT               C          1           Administrative field -- no data
  DSID             I          4           
  CITREF           C          30          Citation Reference
  ---------------- ---------- ----------- ----------------------------------------------------------------------------------------------------------------------------------------

*Style File (ST.DBF)*

  ---------------- ---------- ----------- -----------------------------------------------------------------------------------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  STYLEID          I          4           . of the Style.
  ST_DISPLAY       M          4           The sequence of data entry fields for this style. Individual field names correspond to npt.value (for name styles) or ppt.value (for place styles).
  ST_OUTPUT        M          4           Output template for this style
  GROUP            C          1           Style type (N=Name, P=Place)
  SRNAMESORT       M          4           Surname sort template
  SRNAMEDISP       M          4           Surname display template
  GVNAMESORT       M          4           Given name sort template
  GVNAMEDISP       M          4           Given name display template
  OTHERDISP        M          4           Children/Sibling display template
  TT               C          1           Administrative field -- no data
  DSID             I          4           
  STYLENAME        C          100         Style name
  ---------------- ---------- ----------- -----------------------------------------------------------------------------------------------------------------------------------------------------

*Tag Type File (T.DBF)*

  ---------------- ---------- ----------- --------------------------------------------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  ISPICKED         L          1           Administrative field -- no data
  DSID             I          4           
  ACTIVE           L          1           Is this tag type displayed on the default list?
  ETYPENUM         I          4           . of the Tag Type.
  ORIGETYPE        I          4           Standardized Tag Type . Used for standard tag types only. Zero for custom tag types.
  ADMIN            N          2           Tag type administrative group
  LDSONLY          L          1           Is this tag type an LDS ordinance?
  ETYPENAME        C          20          Tag type label
  GEDCOM_TAG       C          4           GEDCOM tag used for export
  ISREPORT         L          1           Administrative field -- no data
  TSENTENCE        M          4           Tag type (changed structure in v9.0)
  ABBREV           C          4           Tag type abbreviation
  WITDISP          L          1           Display the tag type label for witnesses? (False uses "Witness")
  PASTTENSE        C          20          Past tense of the tag type
  PRINROLE         L          1           Display roles for principals?
  WITROLE          L          1           Display roles for witnesses?
  MAXYEAR          N          4           Maximum appropriate year for this tag type
  MINYEAR          N          4           Minimum appropriate year for this tag type
  REMINDERS        M          4           Hint Memo (new in v7.1)
  TT               C          1           Administrative field -- no data
  PROPERTIES       M          4           Translations for tag type label, abbreviation, and past tense.
  ---------------- ---------- ----------- --------------------------------------------------------------------------------------

*Source Element File (U.DBF)*

+----------------+----------+-----------+------------------------------+
| **Field Name** | **Type** | **Width** | **Description**              |
+----------------+----------+-----------+------------------------------+
| RECNO          | I        | 4         | . of the Source Element.     |
+----------------+----------+-----------+------------------------------+
| DSID           | I        | 4         |                              |
+----------------+----------+-----------+------------------------------+
| ELEMENT        | C        | 30        | Source Element name          |
+----------------+----------+-----------+------------------------------+
| GROUPNUM       | N        | 3         | Source Element               |
|                |          |           | administrative group:        |
|                |          |           |                              |
|                |          |           | 1 Title                      |
|                |          |           |                              |
|                |          |           | 2 Short title                |
|                |          |           |                              |
|                |          |           | 3 Subtitle                   |
|                |          |           |                              |
|                |          |           | 4 Short subtitle             |
|                |          |           |                              |
|                |          |           | 5 Record Type                |
|                |          |           |                              |
|                |          |           | 6 Author                     |
|                |          |           |                              |
|                |          |           | 7 Subject                    |
|                |          |           |                              |
|                |          |           | 8 Compiler                   |
|                |          |           |                              |
|                |          |           | 9 Editor                     |
|                |          |           |                              |
|                |          |           | 10 Second Person             |
|                |          |           |                              |
|                |          |           | 11 Date                      |
|                |          |           |                              |
|                |          |           | 12 Second Date               |
|                |          |           |                              |
|                |          |           | 13 Location                  |
|                |          |           |                              |
|                |          |           | 14 Second Location           |
|                |          |           |                              |
|                |          |           | 15 Publisher                 |
|                |          |           |                              |
|                |          |           | 16 Publisher Location        |
|                |          |           |                              |
|                |          |           | 17 Series                    |
|                |          |           |                              |
|                |          |           | 18 Edition                   |
|                |          |           |                              |
|                |          |           | 19 Version                   |
|                |          |           |                              |
|                |          |           | 20 Volume(s)                 |
|                |          |           |                              |
|                |          |           | 21 Page(s)                   |
|                |          |           |                              |
|                |          |           | 22 Repository                |
|                |          |           |                              |
|                |          |           | 23 Repository Info/Location  |
|                |          |           |                              |
|                |          |           | 24 Repository Reference      |
|                |          |           |                              |
|                |          |           | 25 File Reference            |
|                |          |           |                              |
|                |          |           | 26 File/Record Number        |
|                |          |           |                              |
|                |          |           | 27 Film Number               |
|                |          |           |                              |
|                |          |           | 28 Comments                  |
|                |          |           |                              |
|                |          |           | 29 Citation Detail\          |
|                |          |           | 30 Citation reference\       |
|                |          |           | 31 Citation Memo\            |
|                |          |           | 32 Repository Memo           |
+----------------+----------+-----------+------------------------------+
| TT             | C        | 1         | Administrative field -- no   |
|                |          |           | data                         |
+----------------+----------+-----------+------------------------------+

*Repository Link File (W.DBF)*

Links between sources and respositories.

  ---------------- ---------- ----------- -------------------------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  MNUMBER          I          4           of the linked Source. Related to m.majnum.
  RNUMBER          I          4           of the linked Repository. Related to r.recno.
  REFERENCE        C          25          Repository/Source reference value
  PRIMARY          L          1           Is this the primary repository for this source?
  TT               C          1           Administrative field -- no data
  DSID             I          4           
  ---------------- ---------- ----------- -------------------------------------------------

*Excluded Pair File (XD.DBF)*

People who have been marked as "not a duplicate" in the "Check for
Duplicate People" feature.

  ---------------- ---------- ----------- ---------------------------------
  **Field Name**   **Type**   **Width**   **Description**
  DSID             I          4           
  PER1             I          4           of one person
  PER2             I          4           of another person
  TT               C          1           Administrative field -- no data
  ---------------- ---------- ----------- ---------------------------------

Special Field Values

*Dates*

Date fields contain a structured value as follows:

+--------------------+--------------+------------+--------------------+
| **Irregular        |              |            |                    |
| Dates**            |              |            |                    |
+--------------------+--------------+------------+--------------------+
|                    | **Position** | **Value**  | **Meaning**        |
+--------------------+--------------+------------+--------------------+
|                    | 1            | "0"        | Irregular date     |
|                    |              |            | code               |
+--------------------+--------------+------------+--------------------+
|                    | 2-30         | (text)     | Irregular date     |
|                    |              |            | value              |
+--------------------+--------------+------------+--------------------+
| \                  |              |            |                    |
| **Regular Dates**  |              |            |                    |
+--------------------+--------------+------------+--------------------+
|                    | **Position** | **Value**  | **Meaning**        |
+--------------------+--------------+------------+--------------------+
|                    | 1            | "1"        | Regular date code  |
+--------------------+--------------+------------+--------------------+
|                    | 2-9          | "YYYYMMDD" | Regular date value |
+--------------------+--------------+------------+--------------------+
|                    | 10           | "0"        | Not Old Style      |
+--------------------+--------------+------------+--------------------+
|                    |              | "1"        | Old Style          |
+--------------------+--------------+------------+--------------------+
|                    | 11           | "0"        | Before date        |
+--------------------+--------------+------------+--------------------+
|                    |              | "1"        | Say date           |
+--------------------+--------------+------------+--------------------+
|                    |              | "2"        | Circa date         |
+--------------------+--------------+------------+--------------------+
|                    |              | "3"        | Exact date         |
+--------------------+--------------+------------+--------------------+
|                    |              | "4"        | After date         |
+--------------------+--------------+------------+--------------------+
|                    |              | "5"        | Between date       |
+--------------------+--------------+------------+--------------------+
|                    |              | "6"        | Or date            |
+--------------------+--------------+------------+--------------------+
|                    |              | "7"        | From...to date     |
+--------------------+--------------+------------+--------------------+
|                    | 12-19        | "00000000" | Used for before,   |
|                    |              |            | say, circa, exact, |
|                    |              |            | and after dates.   |
+--------------------+--------------+------------+--------------------+
|                    |              | "YYYYMMDD" | Second date for    |
|                    |              |            | between, or, and   |
|                    |              |            | from/to dates.     |
+--------------------+--------------+------------+--------------------+
|                    | 20           | "0"        | Used for before,   |
|                    |              |            | say, circa, exact, |
|                    |              |            | and after dates.   |
+--------------------+--------------+------------+--------------------+
|                    | 21           | "0"        | No question mark   |
+--------------------+--------------+------------+--------------------+
|                    |              | "1"        | Question mark      |
+--------------------+--------------+------------+--------------------+
|                    | 22-30        | (reserved) | (reserved)         |
+--------------------+--------------+------------+--------------------+

Date Examples:

  ---------------------------- ---------------------------
  **Stored as**                **Displayed As**
  "0Third Monday in January"   "Third Monday in January"
  "119610924000000000000 "     "Before 09 Sep 1961"
  "117120100130000000000 "     "Jan 1712/13"
  "119420000051943000000 "     "Between 1942 and 1943"
  "100000000030000000000 "     (empty date)
  ---------------------------- ---------------------------

*ID Numbers*

As referenced in this document, "**ID Numbers**" are internal integer
values (or "keys") which the program uses to link records between
tables. ID Numbers are typically unique and cannot be seen or modified
by the user within TMG. They are therefore merely administrative in
nature and don't have any inherent genealogical meaning nor are they be
meaningful to users.

Rather, users know their people, sources, and repositories by their
"**User-Defined ID numbers**." These are the numbers that are visible to
users on the screen in TMG and in reports (e.g., "Annie Eliza Alexander
**(6)**"). Although assigned automatically by the program during data
entry (or preserved from the original during an import), users can
modify the User-Defined ID Numbers. The renumbering features within TMG
typically only affect these User-Defined ID Numbers. Note that
User-Defined ID Numbers *are not necessarily unique* except when paired
with a Data Set ID as described below.

A TMG project may contain multiple distinct record collections (or "data
sets") each of which is identified by a unique **Data Set ID**. A data
set typically represents the product of a single researcher or research
effort. Data Sets are distinguished and documented in the Data Set File
(d.dbf).

It is not necessary for all of the records within a data set to be
linked. For instance, a data set may contain individuals or small groups
of people who are unrelated to the main group but which contain the same
Data Set ID. Since data sets are strictly independent, however, records
which have *different* Data Set IDs may [not]{.underline} be linked to
each other.

A typical project contains only one data set. In that case, the Data Set
ID is not shown to the user in TMG. For instance:

Annie Eliza Alexander (6)

When a second file is imported to a project, for instance, it will form
a second [data set]{.underline} within that project (until/unless merged
by the user). In that case, numbered records (people, sources, and
repositories) are identified to the user first by the Data Set ID
followed by a colon and the User-Defined ID Number. For instance:

Annie Eliza Alexander (2:6)

Finally, **Reference Values** are text fields with which users can
supplement or comment about the identity of a record. The researcher
might use the Reference Value for a person to record an Ancestral File
Number, Ahnentafel Number, Social Security Number, Henry Number, or a
reference to his/her file system, for instance. No validation is
performed on Reference Values at the point of data entry so they are not
necessarily unique.

*Sentence Structures*

(changed structure in v8.05)

"Sentence structures" are the templates that the program uses to build
narratives about each event. They can be defined for all tag types of a
specific type (i.e., globally as stored in t.tsentence) or for a
specific event (i.e., locally as stored in e.wsentence).

The internal storage of sentence structures is outlined below.
**Carriage returns and indentation are for clarity here only and will
not be found in the data base.**

The **t.tsentence** field consists of two parts\
1) Description of role names\
2) Sentence

In description of role names part:

\[LABELS:\]

\[RL= *RoleID*\]\[L=*LanguageValue*\] *RoleValue*

\[:LABELS\]

In sentence part (for all events of a specific type):

\[L=*LanguageValue*\]

\[R=*RoleID*\]

\[C=*AppropriateSex*,*MinimumAge*,*MaximumAge*\]

*Sentence structure value*

\[R=*RoleID*\]

\[C=*AppropriateSex*,*MinimumAge*,*MaximumAge*\]

*Sentence structure value*

*etc.*

\[L=*LanguageValue*\]

\[R=*RoleID*\]

\[C=*AppropriateSex*,*MinimumAge*,*MaximumAge*\]

*Sentence structure value*

\[R=*RoleID*\]

\[C=*AppropriateSex*,*MinimumAge*,*MaximumAge*\]

*Sentence structure value*

*etc.*

Any number of supported languages can be represented within the same
field. Each language may have any number of supported roles. The
consistency rules (\[C=...\]) section is optional in each case.

\
The *Sentence structure value* may itself be separated into male and
female sections, delimited by "\$!&".

In e.wsentence (for a witness who is assigned a specific role):

\[L=*LanguageValue*\]

*Sentence structure value*

\[L=*LanguageValue*\]

*Sentence structure value*

*etc.*

Any number of supported languages can be represented within the same
field.

The local witness sentence structure field (e.wsentence) may be empty.
**If it is not empty, however, its contents override the sentence
structure defined for that event type and role in t.tsentence.**

*Surety*

Surety fields may contain character values of 0, 1, 2, 3 (low to high
surety), or a minus sign (-) to represent negative surety. "Max surety"
fields are the highest surety in the appropriate category from all
citations for that record.

*Lat/Long structure*

Lat/Long part of place (pd.value) contains two values and comma between
them:

Lat/Long Examples:

  --------------------------------- -------------------------------------
  **Stored as**                     **Displayed As**
  36.3511111111,-82.2161111111      36.351111,-82.2161111
  36.3511111111,-82.2161111111      36.351111°,-82.2161111°
  36.3511111111,-82.2161111111      36° 21\' 4.12\" , -82° 12\' 57.60\"
  36.3511111111,-82.2161111111      362100N0821257W
  -\\36.3511111111,-82.2161111111   
  --------------------------------- -------------------------------------
