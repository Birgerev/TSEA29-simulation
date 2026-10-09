using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class PIDVisualizer
{
	[Tooltip("Seconds of history shown in the graphs.")]
	public float window = 5f;
	[Tooltip("Graph width in meters.")]
	public float width = 0.4f;
	[Tooltip("Height of each graph in meters.")]
	public float height = 0.15f;
	[Tooltip("Vertical space between the two graphs in meters.")]
	public float gap = 0.03f;
	[Tooltip("The P/I/D/output graph shows -outputRange to +outputRange, in duty units.")]
	public float outputRange = 1f;
	[Tooltip("The error graph shows -errorRange to +errorRange, in the error's units (degrees for heading). Values outside are pinned to the edge.")]
	public float errorRange = 45f;

	[Header("Background and title")]
	[Tooltip("Title drawn above the graphs.")]
	public string title = "PID";
	[Tooltip("Background color behind the graphs. Alpha controls how much of the scene shows through.")]
	public Color backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.8f);
	[Tooltip("Margin around the graphs in meters.")]
	public float padding = 0.02f;
	[Tooltip("Space above the top graph for the title, in meters.")]
	public float titleSpace = 0.03f;
	[Tooltip("Extra background width to the right of the graphs, for the legend, in meters.")]
	public float legendSpace = 0.07f;

	PIDSample[] samples;
	float[] times;
	int head, count;

	// Set per draw call so DrawFrame and DrawSignal can reach them
	Vector3 right, up;
	float tNow;

#if UNITY_EDITOR
	static GUIStyle titleStyle;
#endif

	// Call from FixedUpdate, right after pid.Step()
	public void Update(PID pid)
	{
		int capacity = Mathf.Max(2, Mathf.CeilToInt(window / Time.fixedDeltaTime) + 1);
		if (samples == null || samples.Length != capacity)
		{
			samples = new PIDSample[capacity];
			times = new float[capacity];
			head = 0;
			count = 0;
		}

		// At most one sample per physics step, even if called more often (e.g. from Update)
		if (count > 0 && times[(head - 1 + capacity) % capacity] == Time.fixedTime) return;

		samples[head] = pid.Last;
		times[head] = Time.fixedTime;   // same as Time.time inside FixedUpdate
		head = (head + 1) % capacity;
		if (count < capacity) count++;
	}

	// Call from the owner's OnDrawGizmos. origin is the bottom-left corner of the lower graph.
	public void DrawGizmos(Vector3 origin)
	{
		Camera cam = Camera.current;
		if (cam == null || count < 2) return;

		right = cam.transform.right;
		up = cam.transform.up;
		tNow = times[(head - 1 + samples.Length) % samples.Length];

		// Top graph: what each term contributes, in duty units
		Vector3 top = origin + up * (height + gap);
		DrawBackground(origin, cam.transform);
		DrawFrame(top);
		DrawSignal(top, Color.green,  s => s.p,      outputRange);
		DrawSignal(top, Color.red,    s => s.i,      outputRange);
		DrawSignal(top, Color.blue,   s => s.d,      outputRange);
		DrawSignal(top, Color.yellow, s => s.output, outputRange);

		// Bottom graph: the error itself
		DrawFrame(origin);
		DrawSignal(origin, Color.white, s => s.error, errorRange);

#if UNITY_EDITOR
		if (titleStyle == null)
			titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, normal = { textColor = Color.white } };
		Handles.Label(top + up * (height + titleSpace), title, titleStyle);

		Handles.Label(top + right * width + up * height,
			$"P green\nI red\nD blue\nout yellow\n+/-{outputRange}");
		Handles.Label(origin + right * width + up * height,
			$"error white\n+/-{errorRange}");
#endif
	}

	void DrawBackground(Vector3 origin, Transform cam)
	{
#if UNITY_EDITOR
		// Drawn with Handles, not Gizmos: Handles draw immediately in call order, so the labels
		// drawn later land on top. Gizmos are batched and rendered afterwards, which put the
		// background over the text.
		float w = padding + width + legendSpace;
		float h = padding + height + gap + height + titleSpace + padding;

		// Bottom-left corner, pushed slightly away from the camera so the lines draw in front of it
		Vector3 bl = origin - right * padding - up * padding + cam.forward * 0.005f;

		Vector3[] corners = { bl, bl + up * h, bl + up * h + right * w, bl + right * w };
		Handles.DrawSolidRectangleWithOutline(corners, backgroundColor, Color.clear);
#endif
	}

	void DrawFrame(Vector3 o)
	{
		Vector3 w = right * width;
		Vector3 h = up * height;

		Gizmos.color = Color.gray;
		Gizmos.DrawLine(o, o + w);
		Gizmos.DrawLine(o + h, o + w + h);
		Gizmos.DrawLine(o, o + h);
		Gizmos.DrawLine(o + w, o + w + h);
		Gizmos.DrawLine(o + h * 0.5f, o + w + h * 0.5f);   // zero line
	}

	void DrawSignal(Vector3 o, Color color, System.Func<PIDSample, float> get, float range)
	{
		Gizmos.color = color;
		int n = samples.Length;
		Vector3 prev = default;
		bool first = true;

		for (int k = 0; k < count; k++)
		{
			int idx = (head - count + k + n) % n;
			float age = tNow - times[idx];
			if (age > window) continue;

			// Newest sample at the right edge, oldest at the left
			float x = (1f - age / window) * width;
			float y = Mathf.Clamp01((get(samples[idx]) + range) / (2f * range)) * height;

			Vector3 pt = o + right * x + up * y;
			if (!first) Gizmos.DrawLine(prev, pt);
			prev = pt;
			first = false;
		}
	}
}
