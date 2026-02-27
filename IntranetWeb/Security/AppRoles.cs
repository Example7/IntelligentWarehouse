namespace IntranetWeb.Security;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Magazynier = "Magazynier";
    public const string Operator = "Operator";
    public const string Klient = "Klient";
    public const string Client = "Client";

    public const string AdminOnly = Admin;
    public const string AdminMagazynier = Admin + "," + Magazynier;
    public const string AdminMagazynierOperator = Admin + "," + Magazynier + "," + Operator;
    public const string IntranetStaffOnly = AdminMagazynierOperator;
}
