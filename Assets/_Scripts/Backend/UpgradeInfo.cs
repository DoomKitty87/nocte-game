public static class UpgradeInfo
{

  // All values should default to 1 as they are multipliers, but -1 if the upgrade is locked.
  // All speed multpliers are already taken as inverses, just treat them as speed increases instead of time decreases (1+).

  public static int _jumpHeight = -1;
  public static int _sprintSpeed = -1;

  public static int _maxHealth = -1;
  public static int _healthRegen = -1;

  public static int _damage = -1;
  public static int _reloadSpeed = -1;
  public static int _magSize = -1;
  public static int _critChance = -1; // Crit chance is a value from 0-1, only exception for multiplier.

  public static int _scanRange = -1;
  public static int _grappleRange = -1;
  public static int _grappleStrength = -1;
  public static int _scanCooldown = -1;

}