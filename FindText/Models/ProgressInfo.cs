using System;

namespace FindText.Models
{
    /// <summary>
    /// 进度信息类，用于WorkerBase报告任务进度和状态更新。
    /// </summary>
    public class ProgressInfo
    {
        /// <summary>
        /// 进度百分比，范围0-100，表示任务完成的程度
        /// </summary>
        public double Percent { get; set; }      

        /// <summary>
        /// 当前完成数，可以是已处理的文件数、已处理的行数等
        /// </summary>
        public int Current { get; set; }          

        /// <summary>
        /// 总数，可以是总文件数、总行数等
        /// </summary>
        public int Total { get; set; }             

        /// <summary>
        /// 状态消息，可以是当前正在处理的文件名、步骤描述等
        /// </summary>
        public string Message { get; set; }        

        /// <summary>
        /// 开始时间，WorkerBase会自动设置为当前时间，外部不需要设置
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 自定义数据，可以用于传递额外的信息，如当前处理的对象、错误详情等，WorkerBase不会使用这个属性，完全由外部使用者决定如何使用
        /// </summary>
        public object Tag { get; set; }    
    }

}
