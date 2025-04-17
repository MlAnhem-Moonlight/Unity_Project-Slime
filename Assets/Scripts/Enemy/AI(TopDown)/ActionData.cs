// Struct ?? l?u thông tin hành ??ng
[System.Serializable]
public struct ActionData
{
    public string ActionName;
    public float Damage;
    public float Stamina;
    public float PositionAdvantage;

    public ActionData(string name, float damage, float stamina, float positionAdvantage)
    {
        ActionName = name;
        Damage = damage;
        Stamina = stamina;
        PositionAdvantage = positionAdvantage;
    }
}
