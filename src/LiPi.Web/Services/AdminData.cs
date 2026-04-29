namespace LiPi.Web.Services;

/// <summary>
/// Pure UI lookup data — lists of options for dropdowns, datalists, etc.
/// All string lists are alphabetical unless a domain-specific order is intentional.
/// No hard-coded IDs or DB keys live here.
/// </summary>
public static class AdminData
{
    // ── Country calling codes ────────────────────────────────────────────────
    public static readonly (string Code, string Label)[] CountryCodes =
    [
        ("+93",  "+93  Afghanistan"),
        ("+355", "+355 Albania"),
        ("+213", "+213 Algeria"),
        ("+244", "+244 Angola"),
        ("+54",  "+54  Argentina"),
        ("+61",  "+61  Australia"),
        ("+43",  "+43  Austria"),
        ("+880", "+880 Bangladesh"),
        ("+32",  "+32  Belgium"),
        ("+55",  "+55  Brazil"),
        ("+855", "+855 Cambodia"),
        ("+1",   "+1   Canada / USA"),
        ("+56",  "+56  Chile"),
        ("+86",  "+86  China"),
        ("+57",  "+57  Colombia"),
        ("+20",  "+20  Egypt"),
        ("+251", "+251 Ethiopia"),
        ("+33",  "+33  France"),
        ("+49",  "+49  Germany"),
        ("+233", "+233 Ghana"),
        ("+30",  "+30  Greece"),
        ("+91",  "+91  India"),
        ("+62",  "+62  Indonesia"),
        ("+98",  "+98  Iran"),
        ("+964", "+964 Iraq"),
        ("+353", "+353 Ireland"),
        ("+972", "+972 Israel"),
        ("+39",  "+39  Italy"),
        ("+81",  "+81  Japan"),
        ("+962", "+962 Jordan"),
        ("+254", "+254 Kenya"),
        ("+82",  "+82  South Korea"),
        ("+965", "+965 Kuwait"),
        ("+856", "+856 Laos"),
        ("+961", "+961 Lebanon"),
        ("+60",  "+60  Malaysia"),
        ("+52",  "+52  Mexico"),
        ("+212", "+212 Morocco"),
        ("+95",  "+95  Myanmar"),
        ("+977", "+977 Nepal"),
        ("+31",  "+31  Netherlands"),
        ("+64",  "+64  New Zealand"),
        ("+234", "+234 Nigeria"),
        ("+47",  "+47  Norway"),
        ("+968", "+968 Oman"),
        ("+92",  "+92  Pakistan"),
        ("+63",  "+63  Philippines"),
        ("+48",  "+48  Poland"),
        ("+351", "+351 Portugal"),
        ("+974", "+974 Qatar"),
        ("+7",   "+7   Russia"),
        ("+966", "+966 Saudi Arabia"),
        ("+65",  "+65  Singapore"),
        ("+27",  "+27  South Africa"),
        ("+34",  "+34  Spain"),
        ("+94",  "+94  Sri Lanka"),
        ("+46",  "+46  Sweden"),
        ("+41",  "+41  Switzerland"),
        ("+886", "+886 Taiwan"),
        ("+255", "+255 Tanzania"),
        ("+66",  "+66  Thailand"),
        ("+216", "+216 Tunisia"),
        ("+90",  "+90  Turkey"),
        ("+256", "+256 Uganda"),
        ("+971", "+971 UAE"),
        ("+44",  "+44  UK"),
        ("+380", "+380 Ukraine"),
        ("+58",  "+58  Venezuela"),
        ("+84",  "+84  Vietnam"),
        ("+967", "+967 Yemen"),
        ("+263", "+263 Zimbabwe"),
    ];

    // ── Indian states + UTs (alphabetical) ───────────────────────────────────
    public static readonly string[] IndianStates =
    [
        "Andaman & Nicobar Islands",
        "Andhra Pradesh",
        "Arunachal Pradesh",
        "Assam",
        "Bihar",
        "Chandigarh",
        "Chhattisgarh",
        "Dadra & Nagar Haveli and Daman & Diu",
        "Delhi (NCT)",
        "Goa",
        "Gujarat",
        "Haryana",
        "Himachal Pradesh",
        "Jammu & Kashmir",
        "Jharkhand",
        "Karnataka",
        "Kerala",
        "Ladakh",
        "Lakshadweep",
        "Madhya Pradesh",
        "Maharashtra",
        "Manipur",
        "Meghalaya",
        "Mizoram",
        "Nagaland",
        "Odisha",
        "Puducherry",
        "Punjab",
        "Rajasthan",
        "Sikkim",
        "Tamil Nadu",
        "Telangana",
        "Tripura",
        "Uttar Pradesh",
        "Uttarakhand",
        "West Bengal",
        "— Outside India —",
    ];

    // ── Major Indian cities (alphabetical) ───────────────────────────────────
    public static readonly string[] MajorCities =
    [
        "Agra", "Ahmedabad", "Ajmer", "Aligarh", "Allahabad (Prayagraj)",
        "Amravati", "Amritsar", "Asansol", "Aurangabad", "Bareilly",
        "Belagavi (Belgaum)", "Bengaluru", "Bhavnagar", "Bhilai", "Bhopal",
        "Bhubaneswar", "Bikaner", "Chandigarh", "Chennai", "Coimbatore",
        "Cuttack", "Dehradun", "Delhi", "Dhanbad", "Durgapur",
        "Erode", "Faridabad", "Firozabad", "Gaya", "Ghaziabad",
        "Gorakhpur", "Gurugram", "Guwahati", "Gwalior", "Howrah",
        "Hubballi-Dharwad", "Hyderabad", "Indore", "Jabalpur", "Jaipur",
        "Jalandhar", "Jalgaon", "Jamnagar", "Jamshedpur", "Jhansi",
        "Jodhpur", "Kalaburagi (Gulbarga)", "Kanpur", "Kochi", "Kolhapur",
        "Kolkata", "Kota", "Lucknow", "Ludhiana", "Madurai",
        "Mangaluru", "Meerut", "Moradabad", "Mumbai", "Mysuru",
        "Nagpur", "Nashik", "Navi Mumbai", "Nellore", "Noida",
        "Patna", "Pimpri-Chinchwad", "Pune", "Raipur", "Rajkot",
        "Ranchi", "Rourkela", "Saharanpur", "Salem", "Siliguri",
        "Solapur", "Srinagar", "Surat", "Thane", "Thiruvananthapuram",
        "Tiruchirappalli", "Tirunelveli", "Udaipur", "Ujjain", "Vadodara",
        "Varanasi", "Vijayawada", "Visakhapatnam", "Warangal",
    ];

    // ── Titles ───────────────────────────────────────────────────────────────
    public static readonly string[] Titles =
    [
        "Dr.", "Mr.", "Ms.", "Mrs.", "Prof.", "Sr.", "Br.", "Rev.", "Other"
    ];

    // ── Qualifications (alphabetical) ────────────────────────────────────────
    public static readonly string[] Qualifications =
    [
        "ANM",
        "B.Pharm",
        "B.Sc MLT",
        "B.Sc Nursing",
        "B.Sc Radiation Therapy",
        "B.Sc Radiology / Imaging",
        "BAMS",
        "BDS",
        "BHMS",
        "BNYS",
        "DM",
        "DMLT",
        "DMRT",
        "DNB",
        "DNB Medical Oncology",
        "DNB Radiation Oncology",
        "FRCS",
        "GNM",
        "M.Pharm",
        "M.Sc Medical Imaging",
        "M.Sc MLT",
        "M.Sc Nursing",
        "MBA Hospital Administration",
        "MBBS",
        "MCh",
        "MD",
        "MD Medical Oncology",
        "MD Radiation Oncology",
        "MDS",
        "MHA",
        "MRCP",
        "MS",
        "MSc Medical Physics",
        "PG Diploma Healthcare Mgmt",
        "PGDMRT",
        "Pharm.D",
        "PhD Medical Physics",
    ];

    // ── Designations (alphabetical) ──────────────────────────────────────────
    public static readonly string[] Designations =
    [
        "Administrator",
        "Associate Consultant",
        "Attending Physician",
        "CEO / COO",
        "Charge Nurse / Head Nurse",
        "Consultant",
        "Dean",
        "Department Manager",
        "Departmental Director",
        "Director",
        "Dosimetrist",
        "Front Office Executive",
        "Head of Department",
        "House Officer / Intern",
        "Junior Nurse",
        "Junior Physicist",
        "Lab Technician",
        "Medical Director",
        "Medical Oncologist",
        "Medical Physicist",
        "Nuclear Medicine Physician",
        "Pharmacist",
        "Pharmacy Assistant",
        "Radiation Oncologist",
        "Radiation Therapist",
        "Radiographer",
        "Radiologist",
        "Registrar",
        "Resident Medical Officer",
        "Senior Consultant",
        "Senior Dosimetrist",
        "Senior House Officer",
        "Senior Lab Technician",
        "Senior Medical Physicist",
        "Senior Pharmacist",
        "Senior Radiation Therapist",
        "Senior Radiographer",
        "Senior Registrar",
        "Senior Staff Nurse",
        "Staff Nurse",
        "Surgeon",
        "Surgical Oncologist",
        "Visiting Consultant",
    ];

    // ── Organization types (alphabetical) ────────────────────────────────────
    public static readonly (string Value, string Label)[] OrgTypes =
    [
        ("academic",       "Academic / Research"),
        ("corporate",      "Corporate / Pvt Ltd"),
        ("government",     "Government"),
        ("hospital_group", "Hospital Group"),
        ("ngo",            "NGO / Society"),
        ("single_clinic",  "Single Clinic"),

        ("trust",          "Charitable Trust"),
    ];

    // ── Clinic types (alphabetical) ──────────────────────────────────────────
    public static readonly (string Value, string Label)[] ClinicTypes =
    [
        ("cancer_centre", "Cancer Centre"),
        ("clinic",        "Clinic"),
        ("day_care",      "Day Care Centre"),
        ("diagnostic",    "Diagnostic Centre"),
        ("hospital",      "Hospital"),
        ("polyclinic",    "Polyclinic"),
    ];

    // ── Staff types (alphabetical) ────────────────────────────────────────────
    public static readonly (string Value, string Label)[] StaffTypes =
    [
        ("admin",               "Admin / Front Office"),
        ("billing",             "Billing Staff"),
        ("director",            "Director"),
        ("doctor",              "Doctor / Physician"),
        ("dosimetrist",         "Dosimetrist"),
        ("lab_tech",            "Lab Technician"),
        ("manager",             "Manager"),
        ("medical_oncologist",  "Medical Oncologist"),
        ("medical_physicist",   "Medical Physicist"),
        ("nuclear_med",         "Nuclear Medicine"),
        ("nurse",               "Nurse"),
        ("pharmacist",          "Pharmacist"),
        ("radiation_oncologist","Radiation Oncologist"),
        ("radiation_therapist", "Radiation Therapist"),
        ("radiographer",        "Radiographer / RTT"),
        ("radiologist",         "Radiologist"),
        ("support",             "Support Staff"),
        ("surgeon",             "Surgeon"),
    ];

    // ── Gender options ────────────────────────────────────────────────────────
    public static readonly (string Value, string Label)[] Genders =
    [
        ("Female",           "Female"),
        ("Male",             "Male"),
        ("Other",            "Other"),
        ("Prefer not to say","Prefer not to say"),
    ];

    // ── Blood groups (standard clinical order) ────────────────────────────────
    public static readonly string[] BloodGroups =
        ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-", "Unknown"];

    // ── Timezones relevant to India + common global ──────────────────────────
    public static readonly (string Value, string Label)[] Timezones =
    [
        ("Asia/Kolkata",      "IST — India Standard Time (UTC+5:30)"),
        ("Asia/Colombo",      "Sri Lanka Time (UTC+5:30)"),
        ("Asia/Kathmandu",    "Nepal Time (UTC+5:45)"),
        ("Asia/Dhaka",        "Bangladesh Time (UTC+6:00)"),
        ("Asia/Dubai",        "UAE / Gulf Time (UTC+4:00)"),
        ("Asia/Singapore",    "Singapore / Malaysia (UTC+8:00)"),
        ("Asia/Tokyo",        "Japan Standard Time (UTC+9:00)"),
        ("Australia/Sydney",  "AEDT — Eastern Australia (UTC+11:00)"),
        ("Europe/London",     "GMT/BST — United Kingdom"),
        ("Europe/Berlin",     "CET — Central Europe"),
        ("America/New_York",  "EST/EDT — US Eastern"),
        ("America/Chicago",   "CST/CDT — US Central"),
        ("America/Los_Angeles","PST/PDT — US Pacific"),
        ("UTC",               "UTC (Coordinated Universal Time)"),
    ];

    // ── Module definitions for top nav ────────────────────────────────────────
    public sealed record ModuleDef(string Code, string Name, string Color, string Route);

    public static readonly ModuleDef[] ClinicalModules =
    [
        new("PT",  "Patients",            "#1D9E75", "/patients"),
        new("OP",  "Outpatient",          "#4A9BD4", "/module/op"),
        new("IP",  "Inpatient",           "#3b82f6", "/module/ip"),
        new("RO",  "Radiation Oncology",  "#a855f7", "/module/ro"),
        new("MO",  "Medical Oncology",    "#f97316", "/module/mo"),
        new("Sx",  "Surgery",             "#ef4444", "/module/sx"),
        new("CV",  "Cardiovascular",      "#0ea5e9", "/module/cv"),
    ];

    public static readonly ModuleDef[] SupportModules =
    [
        new("BL",  "Billing",    "#C49A22", "/module/billing"),
        new("Ph",  "Pharmacy",   "#22c55e", "/module/ph"),
        new("Lab", "Laboratory", "#eab308", "/module/lab"),
        new("Rad", "Radiology",  "#67e8f9", "/module/rad"),
        new("NM",  "Nuclear Med","#f59e0b", "/module/nm"),
    ];

    // ── State → cities mapping (for PIN/city/state auto-fill) ────────────────
    public static readonly Dictionary<string, string[]> StateCities = new()
    {
        ["Maharashtra"]      = ["Mumbai","Navi Mumbai","Thane","Pune","Pimpri-Chinchwad","Nagpur","Nashik","Aurangabad","Solapur","Kolhapur","Amravati","Jalgaon","Akola","Latur"],
        ["Delhi (NCT)"]      = ["Delhi","New Delhi","Noida","Gurgaon","Faridabad","Ghaziabad"],
        ["Karnataka"]        = ["Bengaluru","Mysuru","Mangaluru","Hubballi","Belagavi","Kalaburagi","Davangere","Ballari","Shivamogga","Tumakuru"],
        ["Tamil Nadu"]       = ["Chennai","Coimbatore","Madurai","Tiruchirappalli","Salem","Tirunelveli","Erode","Vellore","Thoothukudi","Tiruppur"],
        ["Telangana"]        = ["Hyderabad","Warangal","Nizamabad","Khammam","Karimnagar","Ramagundam","Mahbubnagar"],
        ["Gujarat"]          = ["Ahmedabad","Surat","Vadodara","Rajkot","Bhavnagar","Jamnagar","Junagadh","Gandhinagar","Anand"],
        ["Rajasthan"]        = ["Jaipur","Jodhpur","Udaipur","Kota","Bikaner","Ajmer","Bhilwara","Alwar","Bharatpur","Sikar"],
        ["Uttar Pradesh"]    = ["Lucknow","Kanpur","Agra","Varanasi","Meerut","Allahabad","Ghaziabad","Noida","Gorakhpur","Aligarh","Moradabad","Bareilly","Saharanpur"],
        ["West Bengal"]      = ["Kolkata","Howrah","Durgapur","Asansol","Siliguri","Bardhaman","Malda","Kharagpur"],
        ["Kerala"]           = ["Thiruvananthapuram","Kochi","Kozhikode","Thrissur","Kannur","Kollam","Palakkad","Malappuram"],
        ["Punjab"]           = ["Ludhiana","Amritsar","Jalandhar","Patiala","Bathinda","Mohali","Chandigarh"],
        ["Bihar"]            = ["Patna","Gaya","Bhagalpur","Muzaffarpur","Darbhanga","Purnia","Ara"],
        ["Madhya Pradesh"]   = ["Bhopal","Indore","Jabalpur","Gwalior","Ujjain","Rewa","Sagar","Satna","Dewas"],
        ["Andhra Pradesh"]   = ["Visakhapatnam","Vijayawada","Guntur","Nellore","Kurnool","Tirupati","Rajahmundry","Kakinada"],
        ["Haryana"]          = ["Gurugram","Faridabad","Panipat","Ambala","Hisar","Karnal","Rohtak","Sonipat","Yamunanagar"],
        ["Odisha"]           = ["Bhubaneswar","Cuttack","Rourkela","Berhampur","Sambalpur","Puri"],
        ["Jharkhand"]        = ["Ranchi","Jamshedpur","Dhanbad","Bokaro","Deoghar","Hazaribagh"],
        ["Assam"]            = ["Guwahati","Silchar","Dibrugarh","Jorhat","Nagaon","Tinsukia"],
        ["Chandigarh"]       = ["Chandigarh"],
        ["Goa"]              = ["Panaji","Margao","Vasco da Gama","Mapusa","Ponda"],
        ["Himachal Pradesh"] = ["Shimla","Manali","Dharamshala","Solan","Mandi","Kullu","Hamirpur"],
        ["Uttarakhand"]      = ["Dehradun","Haridwar","Roorkee","Haldwani","Nainital","Rishikesh","Mussoorie"],
        ["Chhattisgarh"]     = ["Raipur","Bhilai","Durg","Bilaspur","Korba","Rajnandgaon"],
        ["Jammu & Kashmir"]  = ["Srinagar","Jammu","Anantnag","Baramulla","Sopore"],
        ["Puducherry"]       = ["Puducherry","Karaikal","Yanam","Mahe"],
        ["Tripura"]          = ["Agartala","Udaipur","Dharmanagar","Kailasahar"],
        ["Manipur"]          = ["Imphal","Thoubal","Bishnupur","Churachandpur"],
        ["Meghalaya"]        = ["Shillong","Tura","Jowai","Nongstoin"],
        ["Sikkim"]           = ["Gangtok","Namchi","Gyalshing","Mangan"],
    };


    // ── Role Codes (all valid role codes in the system) ──────────────────────
    // Mutually exclusive primary groups
    public static readonly string[] PrimaryRoleGroups =
    [
        "clinician","nursing","physicist","dosimetrist","rtt",
        "rad_tech","lab_tech","nm_tech","ot_tech","cssd_tech","billing","pharmacy","dietician","physiotherapist","counsellor"
    ];

    // All primary role codes
    public static readonly string[] AllPrimaryRoleCodes =
    [
        // Clinician
        "consultant","visiting_consultant","medical_officer","senior_resident","junior_resident",
        // Nursing
        "chief_nurse","nurse_manager","nurse_supervisor","staff_nurse","trainee_nurse",
        // Physicist
        "chief_physicist","senior_physicist","medical_physicist","resident_physicist","trainee_physicist",
        // Dosimetrist
        "senior_dosimetrist","dosimetrist",
        // RTT
        "chief_rtt","senior_rtt","rtt","trainee_rtt",
        // Rad Tech
        "chief_rad_tech","senior_rad_tech","rad_tech","trainee_rad_tech",
        // Lab Tech
        "chief_lab_tech","senior_lab_tech","lab_tech","trainee_lab_tech",
        // NM Tech
        "chief_nm_tech","senior_nm_tech","nm_tech","trainee_nm_tech",
        // OT Tech
        "ot_incharge","ot_tech","trainee_ot_tech",
        // CSSD Tech
        "cssd_incharge","cssd_tech","trainee_cssd_tech",
        // Billing
        "head_billing","billing_manager","billing_supervisor","billing_executive","billing_trainee",
        // Pharmacy
        "chief_pharmacist","senior_pharmacist","pharmacist","trainee_pharmacist",
        // Dietician
        "chief_dietician","senior_dietician","dietician","trainee_dietician",
        // Physiotherapist
        "chief_physio","senior_physio","physiotherapist","trainee_physio",
        // Counsellor
        "senior_counsellor","counsellor","social_worker","trainee_counsellor",
    ];

    // Operations & RSO: can be primary-only, add-on, or both
    public static readonly string[] OpsRoleCodes =
    [
        "coo","facility_director","administrator","mro",
        "front_desk_manager","front_desk_executive","help_desk",
    ];
    public static readonly string[] RsoRoleCodes = [ "rso_1","rso_2","rso_3" ];

    // Visiting consultants are excluded from Break Glass access
    public static readonly string[] NoBreakGlassRoles = [ "visiting_consultant" ];


}
