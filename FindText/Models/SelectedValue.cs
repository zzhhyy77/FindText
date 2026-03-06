using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindText.Models
{
    public class SelectedValue : VModelsBase
    {
        bool _isSelected;

        public SelectedValue()
        {
            _isSelected = false;
        }

        public string? Value { get; set; }

        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                this._isSelected = value;
                this.RaisePropertyChanged("IsSelected");
            }
        }
    }
}
