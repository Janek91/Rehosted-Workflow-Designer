﻿using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;

namespace RehostedWorkflowDesigner.CSharpExpressionEditor
{
    internal class ExpressionSelection : ContextItem
    {
        private ModelItem modelItem;

        public ExpressionSelection()
        {
        }

        public ExpressionSelection(ModelItem modelItem)
        {
            this.modelItem = modelItem;
        }

        public ModelItem ModelItem
        {
            get { return modelItem; }
        }

        public override Type ItemType
        {
            get
            {
                return typeof(ExpressionSelection);
            }
        }
    }
}