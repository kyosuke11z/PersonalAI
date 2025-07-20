using System;

namespace PersonalAI
{
    /// <summary>
    /// คลาสสำหรับเก็บข้อมูลความชอบ/ไม่ชอบที่สามารถ binding กับ UI ได้
    /// </summary>
    public class PreferenceItem
    {
        public string Key { get; set; }
        public int Value { get; set; }
        
        public PreferenceItem(string key, int value)
        {
            Key = key;
            Value = value;
        }
    }
    
    /// <summary>
    /// คลาสสำหรับเก็บข้อมูลสุขภาพที่สามารถ binding กับ UI ได้
    /// </summary>
    public class HealthItem
    {
        public string Key { get; set; }
        public bool Value { get; set; }
        
        public HealthItem(string key, bool value)
        {
            Key = key;
            Value = value;
        }
    }
}