namespace TribeOfBattle;

internal sealed class MantisHPFix : MonoBehaviour {
	public void Update() {
		GetComponent<HealthManager>().hp = int.MaxValue;
		UObject.Destroy(this);
	}
}
