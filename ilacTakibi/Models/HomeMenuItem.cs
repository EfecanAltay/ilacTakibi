﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ilacTakibi.Models
{
    public enum MenuItemType
    {
        Browse,
        About
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
