namespace AgroRegionApp
{
    internal static class CompanyProfile
    {
        public const string LegalName = "ООО «Агро Регион»";
        public const string ShortName = "ООО «Агро Регион»";
        public const string Address = "241013, Брянская область, г. Брянск, ул. Кромская, д. 54, офис 1";
        public const string Ogrn = "1193256006437";
        public const string Inn = "3257070615";
        public const string Kpp = "325701001";
        public const string BankName = "Брянское отделение №8605 ПАО Сбербанк";
        public const string BankAccount = "40702810308000011590";
        public const string CorrAccount = "30101810400000000601";
        public const string Bik = "041501601";
        public const string DirectorTitle = "Генеральный директор";
        public const string DirectorName = "Ромашов И.В.";
        public const string DirectorNameFull = "Ромашов Игорь Владимирович";
        public const string City = "г. Брянск Брянской области";

        public static string OrganizationLine =>
            LegalName + ", " + Address + ", ИНН " + Inn + ", КПП " + Kpp;

        public static string BankDetailsLine =>
            "р/с " + BankAccount + " в " + BankName + ", к/с " + CorrAccount + ", БИК " + Bik;

        public static string FullRequisites =>
            LegalName + "\n" + Address + "\nОГРН " + Ogrn + ", ИНН " + Inn + ", КПП " + Kpp + "\n" + BankDetailsLine;
    }
}
