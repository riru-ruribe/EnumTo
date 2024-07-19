using System;
using UnityEngine;

namespace EnumTo.Samples
{
    sealed class EnumToSampleScene : MonoBehaviour
    {
        void Start()
        {
            EnumToNameTest();
            EnumToValueTest1();
            EnumToValueTest2();
            EnumToValueTest3();
            EnumToValueTest4();
            EnumeratorTest();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                EnumToNameTest();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                EnumToValueTest1();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                EnumToValueTest2();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                EnumToValueTest3();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                EnumToValueTest4();
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                EnumeratorTest();
            }
        }

        void EnumToNameTest()
        {
            Debug.Log(Animals.Dog.GetName());
            Debug.Log(Animals.Cat.GetName());
        }

        void EnumToValueTest1()
        {
            UnityEngine.Assertions.Assert.IsTrue(Fruits.Apple.GetValue() == 0);
            UnityEngine.Assertions.Assert.IsTrue(Fruits.Peach.GetValue() == 2);
            UnityEngine.Assertions.Assert.IsTrue(Fruits.Cherry.GetValue() == 4);
            Catch<IndexOutOfRangeException>(() => FruitsTo.GetValue(4)); // 2 is correct. because it is enum value.
        }

        void EnumToValueTest2()
        {
            foreach (var screen in ScreensTo.GetValues())
            {
                screen.Next();
            }
        }

        void EnumToValueTest3()
        {
            foreach (var color in ColorsTo.GetValues())
            {
                Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>ColorsTo</color>");
            }
        }

        void EnumToValueTest4()
        {
            foreach (var color in Color32sTo.GetValues())
            {
                Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>Color32sTo</color>");
            }
        }

        void EnumeratorTest()
        {
            foreach (var _ in AnimalsTo.GetNames()) ;
            foreach (var _ in AnimalsTo.GetValues()) ;
            foreach (var _ in FruitsTo.GetNames()) ;
            foreach (var _ in FruitsTo.GetValues()) ;
            foreach (var _ in ScreensTo.GetNames()) ;
            foreach (var _ in ScreensTo.GetValues()) ;
            foreach (var _ in ColorsTo.GetNames()) ;
            foreach (var _ in ColorsTo.GetValues()) ;
            foreach (var _ in Color32sTo.GetNames()) ;
            foreach (var _ in Color32sTo.GetValues()) ;
        }

        void Catch<T>(Action call) where T : Exception
        {
            try
            {
                call();
            }
            catch (T)
            {
                Debug.Log($"success catch {typeof(T).Name}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
