using Priority_Queue;

public class FastLocation : FastPriorityQueueNode
{
  public readonly int x;
  public readonly int y;

  public FastLocation(int x, int y)
  {
    this.x = x;
    this.y = y;
  }

  public static bool operator ==(FastLocation l1, FastLocation l2)
  {
    return (l1.x == l2.x) && (l1.y == l2.y);
  }

  public static bool operator !=(FastLocation l1, FastLocation l2)
  {
    return !(l1 == l2);
  }

  public bool Equals(FastLocation fast)
  {
    return (fast != null) && (fast == this);
  }

  public static bool Equals(FastLocation l1, FastLocation l2)
  {
    return l1.Equals(l2);
  }

  public override bool Equals(object obj)
  {
    return Equals((FastLocation)obj);
  }

  public override int GetHashCode()
  {
    int hash = 17;
    hash = hash * 23 + x.GetHashCode();
    hash = hash * 23 + y.GetHashCode();
    return hash;
  }
}
