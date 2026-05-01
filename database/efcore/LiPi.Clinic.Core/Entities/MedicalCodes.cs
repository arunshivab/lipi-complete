// ICD-10, SNOMED, LOINC, RxNorm entities are defined for future clinical modules.
// They are NOT registered in ClinicCoreDbContext for the registration phase.
// Add them to ClinicCoreDbContext when clinical coding module is built.
// death_cause_icd10 on Patient is stored as plain text — no FK navigation needed for registration.
namespace LiPi.Clinic.Core.Entities;
