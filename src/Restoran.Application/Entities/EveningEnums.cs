namespace Restoran.Application.Entities;

/// <summary>Õhtu põhjus.</summary>
public enum Occasion
{
    None,
    Birthday,
    Anniversary,
    Proposal,
    Business
}

/// <summary>Peakoka kingitus tähtpäeva puhul (tasuta, üks valik).</summary>
public enum ChefGift
{
    None,
    Cake,
    Champagne,
    Dessert
}

/// <summary>Kelle auks õhtu on.</summary>
public enum HostType
{
    Me,
    Other,
    Social
}

/// <summary>Kuidas personal peakülalise poole pöördub.</summary>
public enum AddressForm
{
    Name,
    Patronymic,
    Formal
}
