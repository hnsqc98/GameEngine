﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.UI.FieldCompleteMainMenu
{
    public class RotateObject : MonoBehaviour
    {
        public float rotSpeed = 2;

        void OnMouseDrag()
        {
            float rotX = Input.GetAxis("Mouse X") * rotSpeed * Mathf.Deg2Rad;
            // Enable this if you want to move Y rotation
            //float rotY = Input.GetAxis("Mouse Y")*rotSpeed*Mathf.Deg2Rad;

            transform.Rotate(Vector3.up, -rotX);
            // Enable this if you want to move Y rotation
            //transform.RotateAround(Vector3.right, rotY);
        }
    }
}