using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [Header("Components")]
    public Transform head;
    public Transform torso;
    public Transform main;
    public Transform hip;
    public Transform[] hand = new Transform[2];
    public Foot[] foot = new Foot[2];

    [Header("Body")]
    [Range(0.15f, 35f)]
    public float WorldScale = 1.25f;
    [Range(0.05f, 3f)]
    public float HeadSize = 1f;
    [Range(1f, 5f)]
    public float LegLength = 1f;
    [Range(0f, 6f)]
    public float ArmLength = 0.5f;
    [Range(0.05f, 1f)]
    public float HandSize = 0.25f;
    [Range(0.5f, 6f)]
    public float HipWidth;
    public Vector3 torsoSize = Vector3.one;
    public Vector3 torsoEulers;
    public Vector3 basePos;
    public Vector3 handPos;
    Vector3 dist;

    [Header("Time Parameter")]
    [Range(0f, 1f)]
    public float TimeScale = 1;
    [Range(0f, 7f)]
    public float speed = 1;
    [Range(1f, 2f)]
    public float AnimationTime = 1;
    public float _DynamicSimilarity;

    [Header("Animation Parameter")]
    [Range(0f, 1f)]
    public float step_formular = 1;
    [Range(0f, 1f)]
    public float step_width = 0;
    [Range(0f, 2f)]
    public float bobbing_perc = 2;
    [Range(0f, 1f)]
    public float crouch_perc = 0;
    [Range(-1f, 1f)]
    public float walking_forward_tilt = 0.3f;
    [Range(-1f, 1f)]
    public float walking_dangle_y = 0.1f;
    [Range(-1f, 1f)]
    public float walking_dangle_z = 0.1f;
    [Range(-1f, 1f)]
    public float walking_dangle_x = 0.1f;
    [Range(-1f, 1f)]
    public float step_weight_dangle_x = 0.1f;
    [Range(0f, 3f)]
    public float hand_stroll = 2f;
    [Range(0f, 3f)]
    public float step_momentum = 0;
    [Range(0f, 1f)]
    public float wobble_amount = 0.2f;

    public bool UpdateFeet = true;
    public bool UpdateBodyMath = true;
    public bool UpdateBodyTransforms = true;
    public bool ShowGUI = true;

    float step_height = 1;
    float step_size = 1;

    //public float test_f;
    //public float test_f2;
    //public float test_f3;

    [Header("Extra")]
    public AudioSource sfx;
    public AudioClip sound;
    public AudioClip sound2;
    bool step0;

    [System.Serializable]
    public class Foot
    {
        public int side;

        public Transform transform;
        public float step_position_x;
        public float y;
        public float z;
        public float x;
        [System.NonSerialized] public float t;
        [System.NonSerialized] public Vector3 pos;
        [System.NonSerialized] public Vector3 roll;

        [System.NonSerialized] public Vector3 rootPosition;

        public void SetMotion(float height, float lift)
        {
            if (t < 1f)
            {
                float a = Mathf.Sin(t * Mathf.PI) * (height + lift);
                pos.y = a;
                pos.z = t;
            }
            else
            {
                //Ground 1f -> 2f ... 0.0 loop
                float tBack = t - 1f;
                pos.z = 1f - tBack;
                pos.y = 0f;
            }
            pos.x = step_position_x * side;
        }

        public void Update(float height, float lift, float step_size, float jogging)
        {
            SetMotion(height, lift);
            transform.localPosition = new Vector3( pos.x + x, pos.y + jogging + y, pos.z * step_size - step_size * 0.5f + jogging + z) + rootPosition;
        }
    }

    private void Update()
    {
        Debug__Bounds__();


        //Start value
        float VAL = 0.375f;
        transform.localScale = Vector3.one * WorldScale;

        _DynamicSimilarity = Mathf.Sqrt(1 / (WorldScale + LegLength * 0.5f));
        _DynamicSimilarity = Mathf.Clamp(_DynamicSimilarity, 0.1f, 1f);


        float moveSpeed = Mathf.Clamp(Mathf.Sqrt(3.5f / speed), 0, 5) * AnimationTime;

        float _LegLength_sqr_val = Mathf.Sqrt(1f / LegLength);
        float _LegLength_sqr = _LegLength_sqr_val * _LegLength_sqr_val;


        //For SFX only
        float weight = Mathf.Sqrt(2 / WorldScale);
        sfx.pitch = Mathf.Clamp(1.7f * weight, 0.4f, 2f);

        float legs_spread_down = 1 - (step_width * step_width * step_width);

        //Time
        foot[0].t += Time.deltaTime * (speed * legs_spread_down + step_momentum) * TimeScale * _DynamicSimilarity * moveSpeed;
        Vector3 local_transform_vector = VAL * speed * speed * legs_spread_down * (LegLength * 1f) * Time.deltaTime * transform.forward * TimeScale * WorldScale * _DynamicSimilarity * moveSpeed;

        //Feet Time Percs, sound effects can be added here
        if (foot[0].t >= 1 && step0 == false) 
        { 
            //sfx.PlayOneShot(sound, 1f);
            step0 = true; 
        }
        if (foot[0].t >= 2) 
        {
            foot[0].t = 0;
            step0 = false; 
            //sfx.PlayOneShot(sound2, 0.5f);
        }
        float check = foot[0].t + step_formular;
        if (check > 2)
        {
            foot[1].t = check - 2;
        }
        else
        {
            foot[1].t = foot[0].t + step_formular;
        }


        //Animation Speed Formular
        step_size = Mathf.Clamp(speed * VAL * LegLength * 1f, 0.5f, 22f);
        step_height = LegLength * 0.1f * speed;


        transform.position += local_transform_vector;//<--Position Update



        //Distance Vectors from Feet Positions are the main source for animations values!
        dist = new Vector3(         Mathf.Abs(foot[0].transform.localPosition.x - foot[1].transform.localPosition.x) - HipWidth, 
                                    foot[0].pos.y - foot[1].pos.y, 
                                    foot[0].transform.localPosition.z - foot[1].transform.localPosition.z);


        float lift_0 = (1 - step_formular) * 0.5f;
        float lift_1 = (1 - step_formular) * 0.5f;
        float distance_0 = foot[0].pos.z - foot[1].pos.z;
        float distance_1 = foot[1].pos.z - foot[0].pos.z;
        if (UpdateFeet)
        {
            //Lifts Up at Start
            float jogg_0 = distance_0 < 0 ? distance_0 * distance_0 * speed * 0.025f * LegLength * Mathf.PI : 0;
            float jogg_1 = distance_1 < 0 ? distance_1 * distance_1 * speed * 0.025f * LegLength * Mathf.PI : 0;
            //Set Feet Data
            foot[0].Update(step_height, lift_0, step_size, jogg_0);
            foot[1].Update(step_height, lift_1, step_size, jogg_1);

            foot[0].rootPosition.x = HipWidth * -0.5f;
            foot[1].rootPosition.x = HipWidth *  0.5f;

            //Spread
            foot[0].step_position_x = step_width * LegLength;//optional: set both step width
            foot[1].step_position_x = step_width * LegLength;//optional: set both step width

            //Foot Roll
            float dy = foot[0].transform.localPosition.y <= 0 ? 0 : -1;
            float dyy = foot[1].transform.localPosition.y <= 0 ? 0 : 1;
            foot[0].transform.localEulerAngles = new Vector3(dist.z * (32f - LegLength * 2) * dy, 0, 0) * _LegLength_sqr;
            foot[1].transform.localEulerAngles = new Vector3(dist.z * (32f - LegLength * 2) * dyy, 0, 0) * _LegLength_sqr;
        }




        //Body Animation/Transform
        float jump_up = Mathf.Sin(foot[0].t * Mathf.PI) * (1 - step_formular) * -0.5f;
        float jump = (foot[0].pos.y - lift_0 + foot[1].pos.y - lift_1) * (1 - step_formular * step_formular);

        if (UpdateBodyMath)
        {

            float crouch = 1 - crouch_perc * 0.314f;
            float bobbing = bobbing_perc - Mathf.Clamp(speed - 3, 0, 0.9f) * Mathf.Clamp(speed - 3, 0, 0.9f);

            float dist_z = Mathf.Abs(dist.z) * 0.1f * _LegLength_sqr;
            float dist_y = dist.y * dist.y * 0.5f * _LegLength_sqr;

            float base_height = (LegLength * crouch + dist_z - dist_y * bobbing) * legs_spread_down;//<--foot[0].pos.z - .. changed to dist.z dist_y needs to fade out to -> running
            

            //float hip_wobble = (foot[1].pos.y - foot[0].pos.y);
            float hip_stroll_y = walking_dangle_y * 20f * dist.y;
            float hip_stroll_z = walking_dangle_z * 20f * dist.y;
            float hip_stroll_x = walking_dangle_x * 20f * Mathf.Abs(dist.y);

            float hip_lean_euler = walking_forward_tilt * 0.1f  * speed * (step_formular);
            float hip_lean_pos = walking_forward_tilt * 0.075f * (LegLength) * speed * (step_formular);


            if (UpdateBodyTransforms)
            {
                //Vector3 local_hip_position = new Vector3(hip_center, hip_height + jump + jump_knee - 0.15f - hip_yz_relative * 0.2f, hip_yz_relative);
                Vector3 local_hip_position = new Vector3(step_weight_dangle_x * dist.z * dist_z, jump + jump_up + base_height, Mathf.Clamp(hip_lean_pos, 0, 99));

                Vector3 local_hip_eulers = new Vector3(hip_lean_euler * 15f + hip_stroll_x, hip_stroll_y, hip_stroll_z);

                main.localPosition = local_hip_position;
                main.localEulerAngles = local_hip_eulers;

                float wobble_value = dist.z * dist.y * wobble_amount * _LegLength_sqr;
                Vector3 wobble = new Vector3( 1,-1, 1) * wobble_value;

                torso.localEulerAngles = local_hip_eulers + torsoEulers;
                torso.localScale = torsoSize + wobble;

                hip.localScale = new Vector3(HipWidth, 0.1f, 0.1f);


                float hands_pos_x = Mathf.Clamp(handPos.x, 0, ArmLength);
                float hands_pos_y = Mathf.Clamp(handPos.x * handPos.x, 0, ArmLength);

                float hand_up = (dist_z * dist_z * dist_z * 25f) + hands_pos_y;

                Vector3 hand_scale_ralative = new Vector3(1 / torsoSize.x, 1 / torsoSize.y, 1 / torsoSize.z);
                hand[0].localScale = hand_scale_ralative * HandSize;
                hand[1].localScale = hand_scale_ralative * HandSize;


                float handStroll = (foot[0].pos.z * -hand_stroll + hand_stroll * 0.5f) * speed * 0.25f * hand_scale_ralative.z;

                hand[0].localPosition = new Vector3( (-torsoSize.x - hands_pos_x) * hand_scale_ralative.x, (torsoSize.y - ArmLength + hand_up) * hand_scale_ralative.y, handStroll);
                hand[1].localPosition = new Vector3(  (torsoSize.x + hands_pos_x) * hand_scale_ralative.x, (torsoSize.y - ArmLength + hand_up) * hand_scale_ralative.y, -handStroll);


                head.localScale = hand_scale_ralative * HeadSize;
                head.localPosition = (1 / torsoSize.y) * 0.5f * Vector3.up;
            }
        }
    }


    void Debug__Bounds__()
    {
        if (Mathf.Abs(transform.position.x) > 500f || Mathf.Abs(transform.position.z) > 500f)
        {
            transform.position = Vector3.zero;
        }
    }

    void TransformGUI()
    {
        Rect area_text = new Rect(new Rect(Screen.width * 0.5f - 170, Screen.height - 40, 350, 100));//(x, y, width, height)
        GUILayout.BeginArea(area_text);
        GUILayout.Label($"Hold Right Mouse Button + Keyboard to move camera https://github.com/jonas-vth");
        GUILayout.EndArea();


        Rect area = new Rect(new Rect(Screen.width - 250, 1, 250, 300));//(x, y, width, height)
        Color oldColor = GUI.color;
        GUI.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        GUI.Box(area, GUIContent.none);
        GUI.color = oldColor;

        GUILayout.BeginArea(area);
        GUILayout.Label($"CHARACTER_PARAMETERS");

        GUILayout.Space(12);
        WorldScale = GUILayout.HorizontalSlider(WorldScale, 0.15f, 35f);
        GUILayout.Label($"WorldScale: {WorldScale:F2}");
        GUILayout.Label($"DynamicSimilarity: {_DynamicSimilarity:F2}");

        torsoSize.x = GUILayout.HorizontalSlider(torsoSize.x, 0.5f, 5f);
        GUILayout.Label($"torsoSize.x: {torsoSize.x:F2}");

        torsoSize.y = GUILayout.HorizontalSlider(torsoSize.y, 0.5f, 5f);
        GUILayout.Label($"torsoSize.y: {torsoSize.y:F2}");

        torsoSize.z = GUILayout.HorizontalSlider(torsoSize.z, 0.5f, 5f);
        GUILayout.Label($"torsoSize.z: {torsoSize.z:F2}");

        LegLength = GUILayout.HorizontalSlider(LegLength, 1f, 5f);
        GUILayout.Label($"LegLength: {LegLength:F2}");
        GUILayout.EndArea();
    }

    void OnGUI()
    {

        if (ShowGUI)
        {
            TransformGUI();

            Rect area = new Rect(new Rect(3, 3, 250, 900));//(x, y, width, height)
            Color oldColor = GUI.color;
            GUI.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            GUI.Box(area, GUIContent.none);
            GUI.color = oldColor;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.padding.left = 20;
            GUI.skin.label = labelStyle;

            GUILayout.BeginArea(area);

            GUILayout.Label($"ANIMATION_PARAMETERS");

            GUILayout.Space(8);
            speed = GUILayout.HorizontalSlider(speed, 0f, 7f);
            GUILayout.Label($"Speed: {speed:F2}");

            GUILayout.Space(12);
            AnimationTime = GUILayout.HorizontalSlider(AnimationTime, 1f, 1.5f);
            GUILayout.Label($"AnimationTime: {AnimationTime:F2}");

            GUILayout.Space(12);
            TimeScale = GUILayout.HorizontalSlider(TimeScale, 0f, 1f);
            GUILayout.Label($"TimeScale: {TimeScale:F2}");

            step_formular = GUILayout.HorizontalSlider(step_formular, 0f, 1f);
            GUILayout.Label($"step_formular: {step_formular:F2}");

            step_width = GUILayout.HorizontalSlider(step_width, 0f, 1f);
            GUILayout.Label($"step_width: {step_width:F2}");

            bobbing_perc = GUILayout.HorizontalSlider(bobbing_perc, 0f, 2f);
            GUILayout.Label($"bobbing_perc: {bobbing_perc:F2}");

            crouch_perc = GUILayout.HorizontalSlider(crouch_perc, 0f, 1f);
            GUILayout.Label($"crouch_perc: {crouch_perc:F2}");

            walking_forward_tilt = GUILayout.HorizontalSlider(walking_forward_tilt, -1f, 1f);
            GUILayout.Label($"walking_forward_tilt: {walking_forward_tilt:F2}");

            walking_dangle_y = GUILayout.HorizontalSlider(walking_dangle_y, -1f, 1f);
            GUILayout.Label($"walking_dangle_y: {walking_dangle_y:F2}");

            walking_dangle_z = GUILayout.HorizontalSlider(walking_dangle_z, -1f, 1f);
            GUILayout.Label($"walking_dangle_z: {walking_dangle_z:F2}");

            walking_dangle_x = GUILayout.HorizontalSlider(walking_dangle_x, -1f, 1f);
            GUILayout.Label($"walking_dangle_x: {walking_dangle_x:F2}");

            step_weight_dangle_x = GUILayout.HorizontalSlider(step_weight_dangle_x, -1f, 1f);
            GUILayout.Label($"step_weight_dangle_x: {step_weight_dangle_x:F2}");

            hand_stroll = GUILayout.HorizontalSlider(hand_stroll, 0f, 2f);
            GUILayout.Label($"hand_stroll: {hand_stroll:F2}");

            step_momentum = GUILayout.HorizontalSlider(step_momentum, 0f, 3f);
            GUILayout.Label($"step_momentum: {step_momentum:F2}");

            wobble_amount = GUILayout.HorizontalSlider(wobble_amount, 0f, 1f);
            GUILayout.Label($"wobble_amount: {wobble_amount:F2}");

            GUILayout.EndArea();
        }
        

    }
}
