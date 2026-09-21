namespace DhhResearch;

internal static class EvidenceCuration
{
    private static readonly string[] Headers = ("effect_id,publication_id,study_id,independence_cluster_id,factor_id,synthesis_id,design,outcome_type,timepoint,intervention,comparator,n_treatment,n_control,mean_treatment,mean_control,sd_treatment,sd_control,score_direction,source_status,source_page_table,eligible,reason,raw_t,raw_df,raw_F,raw_CI_lower,raw_CI_upper,paired_n,paired_r,sd_difference,cluster_count,icc,education_level,sample_size,factor_category,measurement,condition_details,scale_units,scale_min,scale_max,extraction_note,raw_r,correlation_n,raw_md,raw_se,effect_family,pool_role,n_basis,t_direction").Split(',');
    public static int Run(string root)
    {
        var factors = Csv.Read(Path.Combine(root,"data","candidate_factors.csv"));
        if (!factors.Any(x => x["factor_id"] == "reading_proficiency"))
        {
            factors.Add(new() { ["factor_id"]="reading_proficiency",["factor_category"]="Learner",["factor_name"]="Reading proficiency" });
            Csv.Write(Path.Combine(root,"data","candidate_factors.csv"), ["factor_id","factor_category","factor_name"], factors.Select(x=>new object?[]{x["factor_id"],x["factor_category"],x["factor_name"]}));
        }
        var studies = new List<object?[]>();
        void Study(string id, string p, string label, int n, string stage, string site, string page, string note) =>
            studies.Add([id,p,id,label,stage,n,site,"NA",p=="P05"?"separate_school_and_college_samples":site.Contains("RIT")?"cross_report_overlap_unknown":"no_identified_overlap",page,note]);
        Study("P01_E1","P01","caption addition",62,"adult_education","Korean vocational/university courses","PDF 3-6; article 284-287","31 per randomized arm.");
        Study("P02_E1","P02","experiment 1",95,"college","RIT","PDF 6-7; article 426-427","Three arms; arm n missing.");
        Study("P02_E2","P02","experiment 2",60,"college","RIT","PDF 9-10; article 429-430","20 per randomized arm; shared sign comparator.");
        Study("P02_E3","P02","experiment 3",15,"secondary","Australian residential school","PDF 11-12; article 431-432","Within students; delayed missing observations replaced by cell means by original authors.");
        Study("P02_E4","P02","experiment 4",28,"secondary","Netherlands","PDF 12-13; article 432-433","Within-person wording conflicts with Table 4 n=14 per condition.");
        Study("P03_E1","P03","experiment 1",145,"college","RIT","PDF 8-11; article 734-737","DHH sample 145; arm n absent; opening total 187 conflicts with 145+32.");
        Study("P03_E3","P03","experiment 3",22,"college","RIT","PDF 16-25; article 742-751","11 skilled and 11 new signers; two presentations per person. Experiment 2 interpreters is not a DHH student sample.");
        Study("P04_E1","P04","experiment 1",22,"college","RIT","PDF 4; article 549","DHH direct ASL compared with hearing interpreted groups, not a DHH access contrast.");
        Study("P04_E2","P04","experiment 2",24,"college","RIT","PDF 6-8; article 551-553","12 signing and 12 speech-oriented students; incomplete blocks across passage and mode.");
        Study("P04_E3","P04","experiment 3",32,"college","RIT","PDF 9; article 554","Each receives one ASL and one SimCom lecture; per-lecture allocations not printed.");
        Study("P04_E4","P04","experiment 4",22,"college","RIT","PDF 11-12; article 556-557","Counterbalanced different topics; Table 4 posttests adjusted for prior knowledge.");
        Study("P05_HS","P05","high school",48,"secondary","three US high school programs","PDF 4-7; article 55-58","Different participants from college study; 16 per review group; all 48 have reading scores.");
        Study("P05_COL","P05","college",48,"college","RIT","PDF 9-10; article 60-61","Different participants from school study; correlation sample n inferred from complete cohort.");
        Study("P06_E1","P06","expanded captions",9,"secondary","US mainstream schools","PDF 4-9; article 24-29","Methods/Table 1 A=4 B=5 versus Tables 2-3 A=5 B=4.");
        Study("P07_E1","P07","expanded captions",22,"college","RIT","PDF 4-9; article 455-460","Different age/sample from P06; same video materials.");
        Study("P08_E1","P08","caption rate",20,"school","Sydney sign-bilingual school","PDF 4-8; article 155-159","All 20 reading assessments; 3 caption rates on same students.");
        Study("P09_E1","P09","caption editing",15,"primary","five New England schools","PDF 3-7; article 21-25","Ages 7-11; alternating six Arthur episodes; aggregate dependent t.");
        Study("P10_E1","P10","caption rate and language",187,"school","three New York residential schools","PDF 3-5; article 944-946","Three rates x two language levels; per-cell n missing.");
        Study("P11_E1","P11","video and printed captions",50,"school","US schools","PDF 3-6; article 45-48","50 DHH recruited; exact analyzed DHH n unclear; reading correlations not subgroup-specific.");
        Study("P12_A","P12","primary group",7,"primary","Barcelona","PDF 3-9; article 426-432","Cochlear implant users; interviews primarily qualitative.");
        Study("P12_B","P12","older group",20,"secondary","Barcelona","PDF 3-9; article 426-432","Caption condition always second; scored film retelling; indirect reading-association evidence.");
        Study("P13_E1","P13","caption addition",51,"community_mixed_ages","Slovenian deaf clubs","PDF 9-11","Student status not established; ages 15-73; group A 25 B 26.");
        Study("P15_E1","P15","SimCom in quiet",40,"college","RIT","PDF 4-6; article 125-127","Active CIs; incomplete-block passage/mode design; per-passage t degrees of freedom inconsistent.");
        Study("P16_E1","P16","SimCom and noise",48,"college","RIT","PDF 3-6; article 51-54","Active CIs; distinct story fixed to each condition; sequence reversed for 17/48.");
        Study("P17_E1","P17","language modes",25,"secondary","Andalusia/Madrid schools","PDF 4-9","Final 25 DHH after excluding 5 additional participants; within-person discourse conditions.");
        Study("P20_E1","P20","acoustic simulation",18,"primary","Boys Town","PDF 3-6; article 138-141","18 DHH plus 18 hearing; one noise/reverberation condition, not a noise manipulation.");
        Study("P21_E1","P21","print vs interpreted",19,"college","NTID/RIT","PDF 3-4; article 820-821","Within persons; free recall of idea units, a memory proxy.");
        Study("P21_E2","P21","two interpreted lectures",16,"college","NTID/RIT","PDF 5; article 822","Order/serial position comparison not a clean accessibility contrast; overlap with E1 not stated.");
        Study("P22_E1","P22","print vs ASL",20,"college","RIT","PDF 4-6; article 359-361","Different passage per mode, incomplete blocks; not paired same-passage Table 1.");
        Study("P22_E2","P22","input and retelling",23,"college","RIT","PDF 8-11; article 363-366","Explicitly no participants from E1; output groups; NTID reading correlation n=12 for written subgroup.");
        Csv.Write(Path.Combine(root,"data","study_registry.csv"),
            "study_id,publication_id,independence_cluster_id,experiment_label,education_level,n_dhh_unique,recruitment_site,recruitment_dates,overlap_decision,source_page,decision_note".Split(','),studies);
        var registry = studies.ToDictionary(x=>(string)x[0]!,x=>x);
        var rows = new List<Dictionary<string,string>>();
        Dictionary<string,string> Row(string id,string s,string f,string design,string outcome,string trt,string ctrl,string page,bool ok,string note)
        {
            var st=registry[s]; var r=Headers.ToDictionary(k=>k,k=>"NA");
            r["effect_id"]=id; r["publication_id"]=(string)st[1]!; r["study_id"]=s; r["independence_cluster_id"]=s;
            r["factor_id"]=f; r["synthesis_id"]="standalone_"+id; r["design"]=design; r["outcome_type"]=outcome;
            r["timepoint"]="immediate"; r["intervention"]=trt; r["comparator"]=ctrl; r["score_direction"]="higher_better";
            r["source_status"]="pdf_verified"; r["source_page_table"]=page; r["eligible"]=ok?"yes":"no";
            r["reason"]=ok?"standalone_estimate":"incomplete_or_incompatible"; r["extraction_note"]=note; r["condition_details"]=note;
            r["education_level"]=(string)st[4]!; r["sample_size"]=st[5]!.ToString()!;
            r["factor_category"]=factors.Single(x=>x["factor_id"]==f)["factor_category"];
            r["measurement"]=outcome; r["scale_units"]="proportion_correct"; r["scale_min"]="0"; r["scale_max"]="1";
            r["pool_role"]="standalone"; r["n_basis"]="reported_cohort"; rows.Add(r); return r;
        }
        void Values(Dictionary<string,string> r,int? n1,double m1,double sd1,int? n0,double m0,double sd0)
        {
            string F(double x)=>x.ToString(System.Globalization.CultureInfo.InvariantCulture);
            r["n_treatment"]=n1?.ToString()??"NA";r["n_control"]=n0?.ToString()??"NA";
            r["mean_treatment"]=F(m1);r["sd_treatment"]=F(sd1);r["mean_control"]=F(m0);r["sd_control"]=F(sd0);
        }
        var a=Row("E01","P01_E1","captioning","independent_groups","content_comprehension","sign_plus_captions","sign_only","PDF 6; article 287 Table 2",true,"Randomized 31/31; ten multiple-choice questions; selected adult students with limited Korean reading.");
        Values(a,31,73.23,13.26,31,66.13,12.56);a["raw_t"]="-2.16";a["raw_df"]="60";a["scale_units"]="points";a["scale_max"]="100";a["effect_family"]="Hedges_g";
        a=Row("E02","P02_E1","real_time_transcription","independent_groups","content_comprehension","CPrint","sign_interpretation","PDF 7; article 427 Table 1",false,"95 DHH across three arms; no arm counts. No counts inferred from overall ANCOVA degrees of freedom.");
        Values(a,null,.67,.15,null,.53,.20);
        a=Row("E03","P02_E2","real_time_transcription","independent_groups","content_comprehension","CPrint","sign_interpretation","PDF 9-10; article 429 methods and 430 Table 2",true,"20 in each randomized group; immediate 15-item multiple-choice test.");
        Values(a,20,.72,.18,20,.74,.17);a["effect_family"]="Hedges_g";
        a=Row("E04","P02_E2","real_time_transcription","independent_groups","content_comprehension","CART","sign_interpretation","PDF 9-10; article 429 methods and 430 Table 2",true,"Shared control with E03; distinct standalone comparison, never double counted.");
        Values(a,20,.65,.22,20,.74,.17);a["effect_family"]="Hedges_g";
        a=Row("E05","P02_E2","real_time_transcription","independent_groups","content_comprehension","CPrint_with_notes","sign_with_notes","PDF 10; article 430 Table 2",true,"One week plus review; separate timepoint and bundled note access.");
        Values(a,20,.78,.19,20,.76,.17);a["timepoint"]="one_week_with_review";a["effect_family"]="Hedges_g";
        a=Row("E06","P02_E3","real_time_transcription","paired","content_comprehension","CPrint","Auslan","PDF 12; article 432 Table 3",false,"Same 15 students across three modes and different lectures; covariance absent.");
        Values(a,null,.40,.19,null,.44,.20);a["paired_n"]="15";
        a=Row("E07","P02_E3","sign_language_support","paired","content_comprehension","Auslan_plus_CPrint","Auslan","PDF 12; article 432 Table 3",false,"Same cohort as E06; missing paired variance.");
        Values(a,null,.45,.25,null,.44,.20);a["paired_n"]="15";
        a=Row("E08","P02_E4","captioning","ambiguous","content_comprehension","captions_plus_NGT","captions_only","PDF 13; article 433 Table 4",false,"Methods describe one segment each mode but Table 4 n=14 per column for 28 students; no defensible design conversion.");
        Values(a,14,.85,.09,14,.73,.19);
        a=Row("E09","P03_E1","teaching_modality","independent_groups","content_comprehension","live_with_feedback","video_projection","PDF 10; article 736 Table 1",false,"145 DHH total; no arm sample sizes; baseline differences and spontaneous lectures.");
        Values(a,null,.59,.19,null,.53,.20);
        a=Row("E10","P03_E3","teaching_modality","paired","content_comprehension","live","video","PDF 25; article 751 Table 3",false,"11 skilled signers; paired correlation absent; new-signer arm is a separate stratum of same experiment.");
        Values(a,null,.80,.13,null,.72,.22);a["paired_n"]="11";
        a=Row("E11","P04_E3","sign_language_support","incomplete_blocks","content_comprehension","SimCom","ASL","PDF 9; article 554 Table 3",false,"Programming passage: 32 students total, each receives a different lecture in each mode; cell n absent; not same-passage paired data.");
        Values(a,null,.75,.15,null,.76,.14);
        a=Row("E12","P04_E4","teaching_modality","adjusted_incomplete_blocks","content_comprehension","direct_ASL","interpreted","PDF 12; article 557 Table 4",false,"Adjusted posttest means; calculus; per-cell n and adjusted contrast SE absent.");
        Values(a,null,.67,.19,null,.56,.22);
        a=Row("E13","P05_HS","written_materials","independent_groups","content_comprehension","immediate_CPrint_review","no_CPrint_review","PDF 7; article 58 Table 1",true,"Randomly assigned n=16 review groups; same CPrint support, immediate 15-item MC; review group has up to 20 minutes additional study time.");
        Values(a,16,.52,.16,16,.54,.20);a["effect_family"]="Hedges_g";
        a=Row("E14","P05_HS","written_materials","independent_groups","content_comprehension","immediate_interpreter_note_review","no_interpreter_note_review","PDF 7; article 58 Table 1",true,"Same students as E13 in a different support condition; standalone only.");
        Values(a,16,.52,.21,16,.47,.22);a["effect_family"]="Hedges_g";
        void Correlation(string id,string s,double r,int n,string condition,string outcome,string page,string role,string note)
        {
            var x=Row(id,s,"reading_proficiency","correlation",outcome,condition,"reading_proficiency",page,true,note);
            x["raw_r"]=r.ToString(System.Globalization.CultureInfo.InvariantCulture);x["correlation_n"]=n.ToString();x["effect_family"]="Fisher_z";x["pool_role"]=role;
            if(role=="primary"){x["synthesis_id"]="reading_captioned_content";x["timepoint"]="reported_assessment";}
            if(s.StartsWith("P05")){x["n_basis"]="inferred_all_reading_and_outcome_records";x["timepoint"]="mixed_review_timing";}
        }
        Correlation("R01","P05_HS",.51,48,"CPrint","content_comprehension","PDF 4,7; article 55 participant reading scores and 58 correlations","primary","All 48 have reading scores (33 CTBS,15 Mini-Battery); exact pairwise correlation n not separately printed. Outcomes include different review timings.");
        Correlation("R02","P05_COL",.63,48,"CPrint","content_comprehension","PDF 8-10; article 59 participants, 60 procedure and 61 correlations","primary","48 college participants; z-reading scores: 40 California,4 SAT verbal,3 Stanford,1 ACT; reading-to-15-item MC association; pairwise n inferred, not independently printed.");
        Correlation("R03","P08_E1",.75,20,"120_wpm_captions","content_comprehension","PDF 4,8; article 155 participants and 159 Table 1","primary","All 20 assessed; choose middle tested rate before calculating pooled result; mostly short-answer documentary questions.");
        Correlation("R04","P05_HS",.43,48,"interpretation","content_comprehension","PDF 7; article 58 correlations","alternate_support","Same cohort as R01; substitute, never add to it.");
        Correlation("R05","P05_COL",.51,48,"interpretation","content_comprehension","PDF 10; article 61 correlations","alternate_support","Same cohort as R02; substitute, never add.");
        Correlation("R06","P08_E1",.77,20,"90_wpm_captions","content_comprehension","PDF 8; article 159 Table 1","alternate_rate90","Same cohort as R03; substitute, never add.");
        Correlation("R07","P08_E1",.79,20,"180_wpm_captions","content_comprehension","PDF 8; article 159 Table 1","alternate_rate180","Same cohort as R03; substitute, never add.");
        Correlation("R08","P12_B",.544,20,"captioned_fiction","coded_narrative_comprehension","PDF 9; article 432 text beside Table 4","context","Indirect fictional film outcome and five-level reading test; captioned film always shown after uncaptions.");
        Correlation("R09","P22_E2",.61,12,"print_then_written_retelling","retelling_total","PDF 11; article 366 Table 3","context","NTID reading score; subgroup n explicitly 12. Reading-to-reading proxy excluded from lecture/video synthesis and possible RIT overlap.");
        a=Row("E15","P06_E1","captioning","ambiguous_counts","content_comprehension","expanded","standard","PDF 9; article 29 Table 2; compare PDF 5 Table 1",false,"First period Algae; outcome table n=5/4 conflicts with participant table n=4/5. No sample-size repair.");
        Values(a,5,4.80,2.49,4,5.00,2.00);a["scale_units"]="points";a["scale_max"]="10";
        a=Row("E16","P07_E1","captioning","paired","content_comprehension","expanded","standard","PDF 9; article 460 Table 2",false,"Same 22 students across different videos; paired posttest variance absent; interaction F=1.22 is gain difference, not posttest contrast.");
        Values(a,null,6.50,2.15,null,6.36,1.86);a["paired_n"]="22";a["scale_units"]="points";a["scale_max"]="10";a["raw_F"]="1.22";
        a=Row("E17","P08_E1","captioning","reported_contrast","content_comprehension","average_90_and_120_wpm","180_wpm","PDF 7; article 158 planned comparison",true,"Reported repeated-measures contrast SE, not arm SD; SE used directly for MD interval.");
        a["raw_md"]="6.81";a["raw_se"]="2.68";a["raw_df"]="18";a["raw_F"]="6.47";a["raw_CI_lower"]="1.19";a["raw_CI_upper"]="12.44";a["paired_n"]="20";a["scale_units"]="percentage_points";a["scale_max"]="100";a["effect_family"]="MD";
        a=Row("E18","P09_E1","captioning","paired_t","content_comprehension","edited","near_verbatim","PDF 7; article 25 Results",true,"Dependent aggregate t over 15 children; reverse original near-verbatim-minus-edited t=-1.973. Episodes differ.");
        Values(a,null,7.61,2.56,null,7.04,2.33);a["paired_n"]="15";a["raw_t"]="-1.973";a["t_direction"]="comparator_minus_intervention";a["raw_df"]="14";a["effect_family"]="g_z";a["scale_units"]="points";a["scale_max"]="12";
        a=Row("E19","P10_E1","captioning","independent_groups","content_comprehension","language_level1_60wpm","language_level3_60wpm","PDF 5; article 946 Table 1",false,"Total score at 60 wpm; randomized 187 students across six cells; cell n absent.");
        Values(a,null,24.48,3.60,null,21.26,4.60);a["scale_units"]="points";a["scale_max"]="NA";
        a=Row("E20","P10_E1","captioning","independent_groups","content_comprehension","language_level1_90wpm","language_level3_90wpm","PDF 5; article 946 Table 1",false,"Same six-arm study; missing cell n.");
        Values(a,null,22.78,4.19,null,19.90,6.21);a["scale_units"]="points";a["scale_max"]="NA";
        a=Row("E21","P10_E1","captioning","independent_groups","content_comprehension","language_level1_120wpm","language_level3_120wpm","PDF 5; article 946 Table 1",false,"Same six-arm study; missing cell n.");
        Values(a,null,21.21,6.27,null,21.18,5.22);a["scale_units"]="points";a["scale_max"]="NA";
        a=Row("E22","P11_E1","visual_aids","paired","content_comprehension","captioned_video","captions_only","PDF 6; article 48 Table 1",false,"DHH condition means, no condition SD or exact analyzed n. Reported reading r=.78/.73 not explicitly DHH-only, excluded.");
        a["mean_treatment"]="8.05";a["mean_control"]="6.68";a["scale_units"]="points";a["scale_max"]="NA";
        a=Row("E23","P12_B","captioning","paired","coded_intentionality","voice_sound_captions","voice_sound","PDF 8; article 431 narrative sequence results",false,"Reported binary-coded intentionality score; same fictional film second exposure; paired covariance absent.");
        Values(a,null,.70,.470,null,.35,.489);a["paired_n"]="20";
        a=Row("E24","P13_E1","captioning","independent_groups","everyday_content_comprehension","sign_plus_captions","sign_only","PDF 9 methods and 11 Table 5",true,"Hiking first period: A=25 captions, B=26 no captions. Independent within this theme; community sample not established as students; contextual only.");
        Values(a,25,5.28,2.07,26,3.69,1.85);a["effect_family"]="Hedges_g";a["pool_role"]="context";a["scale_units"]="points";a["scale_max"]="10";
        a=Row("E25","P13_E1","captioning","paired","everyday_content_comprehension","captions","no_captions","PDF 10; Table 4",false,"37 deaf subgroup, same individuals; reported caption/no-caption average means; within-person correlation absent.");
        Values(a,null,4.19,2.19,null,3.38,2.00);a["paired_n"]="37";a["scale_units"]="points";a["scale_max"]="10";
        a=Row("E26","P15_E1","sign_language_support","paired_t","content_comprehension","SimCom","speech_only","PDF 6; article 127 overall paired test",true,"Overall t(39)=2.39 across counterbalanced different passages. Do not treat per-passage Table 1 means as within-person same-content scores.");
        a["paired_n"]="40";a["raw_t"]="2.39";a["t_direction"]="intervention_minus_comparator";a["raw_df"]="39";a["effect_family"]="g_z";
        a=Row("E27","P15_E1","sign_language_support","ambiguous_incomplete_blocks","content_comprehension","SimCom_Sea","speech_Sea","PDF 6; article 127 Table 1",false,"40 total; no explicit cell n for this table; published independent-test wording versus df39 inconsistent. Preserve means only.");
        Values(a,null,.79,.15,null,.69,.19);
        a=Row("E28","P16_E1","sign_language_support","paired","content_comprehension","SimCom_noise","speech_noise","PDF 5-6; Tables 2-3, article 53-54",true,"48 repeated participants; actual condition correlation .71; +10 dB babble. Different story fixed to each condition, causal confounding.");
        Values(a,null,5.15,2.72,null,2.81,2.99);a["paired_n"]="48";a["paired_r"]=".71";a["effect_family"]="g_z";a["scale_units"]="points";a["scale_max"]="10";
        a=Row("E29","P16_E1","background_noise","paired","content_comprehension","speech_quiet","speech_noise","PDF 5-6; Tables 2-3, article 53-54",true,"Same students as E28; actual condition correlation .75; fixed passage-condition confound.");
        Values(a,null,5.21,2.80,null,2.81,2.99);a["paired_n"]="48";a["paired_r"]=".75";a["effect_family"]="g_z";a["scale_units"]="points";a["scale_max"]="10";
        a=Row("E30","P17_E1","sign_language_support","paired","discourse_comprehension","sign_supported_speech","speech","PDF 9; discourse results",false,"25 DHH; Friedman/nonparametric comparisons, no paired covariance; language mode effects not significant.");
        Values(a,null,.58,.20,null,.54,.26);a["paired_n"]="25";
        a=Row("E31","P20_E1","background_noise","no_environmental_contrast","content_comprehension","noise_10dB_SNR_RT0.6","NA","PDF 3-5; article 138-140",false,"18 DHH vs 18 hearing under one acoustic setting; hearing-status difference does not estimate noise effect. Means 2.9/5.0 refer different age strata.");
        a["measurement"]="18 factual questions after 10-minute multi-talker play";a["scale_units"]="points";a["scale_max"]="18";
        a=Row("E32","P21_E1","written_materials","paired_F","free_recall","printed_lecture","interpreted_lecture","PDF 4; article 821 Results",true,"Positive print advantage F(1,18)=11.58; t=sqrt(F) valid only for this two-level within-person main contrast; recall proxy separate from MC comprehension.");
        a["paired_n"]="19";a["raw_F"]="11.58";a["raw_df"]="18";a["t_direction"]="intervention_minus_comparator";a["effect_family"]="g_z";
        a=Row("E33","P22_E1","written_materials","incomplete_blocks","content_comprehension","print_Sea","ASL_Sea","PDF 6; article 361 Table 1",false,"20 DHH overall; per-passage arms not explicitly counted; across-mode r is across different passages, unusable for paired same-passage Table 1 contrast.");
        Values(a,null,.82,.13,null,.69,.15);
        Csv.Write(Path.Combine(root,"data","extracted_data.csv"),Headers,rows.Select(r=>Headers.Select(h=>(object?)r[h])));
        var publications=Csv.Read(Path.Combine(root,"data","publications.csv"));
        void Catalog(string id,string authors,string year,string title,string doi,string journal,string vi,string pages,string url,string n,string stage,string design,string outcome,string decision,string tags,string note)
        {
            if(publications.Any(p=>p["publication_id"]==id))return;
            publications.Add(new Dictionary<string,string>{
                ["publication_id"]=id,["authors"]=authors,["year"]=year,["title"]=title,["doi"]=doi,["journal"]=journal,["volume_issue"]=vi,["pages"]=pages,
                ["source_url"]=url,["n_dhh_abstract"]=n,["education_level"]=stage,["study_design_preliminary"]=design,["outcome_preliminary"]=outcome,
                ["screening_status"]=decision,["acquisition_priority"]="D",["access_lead"]="metadata_only",["possible_overlap_family"]="NA",["screening_note"]=note,["factor_tags"]=tags});
        }

        publications.RemoveAll(p=>p["publication_id"]=="P39");
        Catalog("P40","Alasim, K. N.","2021","Recognition and comprehension of multiple-meaning words: Examining a vocabulary intervention with hard of hearing students","10.1353/aad.2021.0030","American Annals of the Deaf","166(3)","262–283","https://pubmed.ncbi.nlm.nih.gov/34719516/","3","primary","replicated_single_case","multiple_meaning_words","candidate_near_scope","reading_proficiency;written_materials","Word-level intervention; excluded from content-comprehension pooling; abstract only.");
        Catalog("P41","Kang, K. Y.; Lederberg, A. R.; Scott, J. A.","2022","Word-level instruction for deaf and hard-of-hearing students: An observation study","10.1093/deafed/enac022","Journal of Deaf Studies and Deaf Education","27(4)","453–467","https://pubmed.ncbi.nlm.nih.gov/35797712/","NA","school","observation","word_instruction_strategies","qualitative_context","reading_proficiency;written_materials","Instructional observation rather than an eligible comprehension contrast; original not supplied.");
        Catalog("P42","Alasmari, A.; Albalhareth, A.","2025","Exploring deaf and hard of hearing students' engagement in reading comprehension strategies in elementary school","10.1353/aad.2025.a985470","American Annals of the Deaf","170(3)","120–140","https://pubmed.ncbi.nlm.nih.gov/41906880/","4","primary","ethnographic_qualitative","reading_strategies","qualitative_context","reading_proficiency;written_materials","Teacher and four first-grade DHH students; qualitative evidence, not a numerical effect.");
        Catalog("P43","Chen, D.; Belote, M.","2025","Facilitating listening skills in children who are visually impaired with additional disabilities or deafblindness","NA","Journal of Visual Impairment & Blindness","119(3)","224–229","https://eric.ed.gov/?id=EJ1477656","NA","NA","practice_report","listening_support","excluded_practice_and_population","background_noise;visual_accessibility","Practice report and deafblind/multiple-disability focus, no DHH student content-comprehension comparison.");
        Catalog("P44","Herring, T. J.; Woolsey, M. L.","2020","Three suggested teaching strategies for students who are deaf or hard of hearing","NA","Support for Learning","35(3)","346–358","https://eric.ed.gov/?id=EJ1264225","NA","NA","descriptive_recommendations","active_student_responding","excluded_no_original_comprehension_effect","visual_aids;teaching_modality","Strategy recommendations; no eligible original comprehension effect in indexed abstract.");
        var simulated=publications.Single(p=>p["publication_id"]=="P37");
        simulated["authors"]="Picou, E. M.; Lewis, D.; Angley, G.; Tharpe, A. M.";simulated["year"]="2020";simulated["doi"]="10.1097/AUD.0000000000000800";
        simulated["journal"]="Ear and Hearing";simulated["volume_issue"]="41(4)";simulated["pages"]="790–803";
        simulated["screening_note"]="21 normally hearing children with simulated unilateral loss; population excluded. Final issue 2020, online first 2019.";
        var supplied=Csv.Read(Path.Combine(root,"data","fulltext_manifest.csv"));
        string[] ph=publications[0].Keys.ToArray();
        foreach(var p in publications)
        {
            if(supplied.Any(x=>x["publication_id"]==p["publication_id"]))
                p["screening_status"]=p["publication_id"]=="P13"?"fulltext_context_nonstudent":"fulltext_assessed";
            else if(p["acquisition_priority"] is "A" or "B")p["screening_status"]="fulltext_unavailable";
            if(p["publication_id"] is "P05" or "P08" or "P12" or "P22" or "P11")
            {if(!p["factor_tags"].Split(';').Contains("reading_proficiency"))p["factor_tags"]+=";reading_proficiency";}
        }
        Csv.Write(Path.Combine(root,"data","publications.csv"),ph,publications.Select(p=>ph.Select(h=>(object?)p[h])));
        Csv.Write(Path.Combine(root,"data","publication_screening.csv"),["publication_id","title_abstract_decision","fulltext_decision","reason"],
            publications.Select(p=>new object?[]{p["publication_id"],supplied.Any(x=>x["publication_id"]==p["publication_id"])?"retrieve_original":p["screening_status"],p["screening_status"],
            supplied.Any(x=>x["publication_id"]==p["publication_id"])?"see study registry and extracted data":p["screening_status"]=="fulltext_unavailable"?"Not supplied after user obtained all purchasable sources; abstract never used for effects":p["screening_note"]}));
        string[] mh=supplied[0].Keys.ToArray();
        foreach(var m in supplied){m["extraction_status"]="methods_results_tables_assessed";m["version"]="user_supplied_published_text";}
        Csv.Write(Path.Combine(root,"data","fulltext_manifest.csv"),mh,supplied.Select(m=>mh.Select(h=>(object?)m[h])));
        Console.WriteLine($"Curated {studies.Count} DHH study units, {rows.Count} extraction rows, {supplied.Count} originals.");
        return 0;
    }
}
