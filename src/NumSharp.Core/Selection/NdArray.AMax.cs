﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NumSharp.Core
{
    public partial class NDArray
    {
        /// <summary>
        /// Return the maximum of an array or minimum along an axis
        /// </summary>
        /// <param name="np"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public NDArray amax(int? axis = null)
        {
            var res = new NDArray(dtype);

            int oldLayout = this.Storage.TensorLayout;

            this.Storage.ChangeTensorLayout(2);
            double[] npArr = this.Storage.GetData<double>();

            if (axis == null)
            {
                double min = npArr[0];
                for (int i = 0; i < npArr.Length; i++)
                    min = Math.Max(min, npArr[i]);

                res.Storage  = new NDStorage(); 
                res.Storage.Allocate(dtype,new Shape(1),1);
                res.Storage.SetData( new double[1] {min});             
            }
            else
            {
                if (axis < 0 || axis >= this.ndim)
                    throw new Exception("Invalid input: axis");
                int[] resShapes = new int[this.shape.Length - 1];
                int index = 0; //index for result shape set
                //axis departs the shape into three parts: prev, cur and post. They are all product of shapes
                int prev = 1;
                int cur = 1;
                int post = 1;
                int size = 1; //total number of the elements for result
                //Calculate new Shape
                for (int i = 0; i < this.shape.Length; i++)
                {
                    if (i == axis)
                        cur = this.shape[i];
                    else
                    {
                        resShapes[index++] = this.shape[i];
                        size *= this.shape[i];
                        if (i < axis)
                            prev *= this.shape[i];
                        else
                            post *= this.shape[i];
                    }
                }
                
                //Fill in data
                index = 0; //index for result data set
                int sameSetOffset = this.Storage.Shape.DimOffset[axis.Value];
                int increments = cur * post;
                double[] resData = new double[size];  //res.Data = new double[size];
                int start = 0;
                double min = 0;
                for (int i = 0; i < this.size; i += increments)
                {
                    for (int j = i; j < i + post; j++)
                    {
                        start = j;
                        min = npArr[start];
                        for (int k = 0; k < cur; k++)
                        {
                            min = Math.Max(min, npArr[start]);
                            start += sameSetOffset;
                        }
                        resData[index++] = min;
                    }
                }
                res.Storage = new NDStorage();
                res.Storage.Allocate(this.dtype, new Shape(resShapes),2); // (resData);
                res.Storage.SetData(resData);
                res.Storage.ChangeTensorLayout(1);
            }
            this.Storage.ChangeTensorLayout(oldLayout);
            return res;
        }
    }
}
