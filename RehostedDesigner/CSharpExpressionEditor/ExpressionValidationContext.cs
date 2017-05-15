using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Hosting;
using System.Activities.Presentation.Model;
using System.Collections.Generic;

namespace RehostedWorkflowDesigner.CSharpExpressionEditor
{
    public class ExpressionValidationContext
    {
        private RoslynExpressionEditor etb;

        public ExpressionValidationContext(RoslynExpressionEditor etb)
        {
            this.etb = etb;
        }

        internal ParserContext ParserContext { get; set; }
        internal Type ExpressionType { get; set; }
        internal string ExpressionText { get; set; }
        internal EditingContext EditingContext { get; set; }
        internal string ValidatedExpressionText { get; set; }
        internal bool UseLocationExpression { get; set; }

        internal void Update(RoslynExpressionEditor _etb)
        {
            EditingContext = _etb.OwnerActivity.GetEditingContext();

            //setup ParserContext
            ParserContext = new ParserContext(_etb.OwnerActivity)
            {
                //callee is a ExpressionTextBox
                Instance = _etb,
                //pass property descriptor belonging to epression's model property (if one exists)
                //TODO: _etb should have expressionModelProperty and the propertyDescriptor should be used here instead of passing null
                PropertyDescriptor = null,
            };

            if (_etb.ExpressionType != null)
            {
                ExpressionType = _etb.ExpressionType;
            }

            ValidatedExpressionText = ExpressionText;
            if (_etb.ExpressionEditorInstance != null)
            {
                ExpressionText = _etb.ExpressionEditorInstance.Text;
            }
            else if (_etb.EditingTextBox != null)
            {
                ExpressionText = _etb.EditingTextBox.Text;
            }
            else
            {
                ExpressionText = _etb.Text;
            }
            UseLocationExpression = _etb.UseLocationExpression;
        }

        internal IEnumerable<string> ReferencedAssemblies
        {
            get
            {
                AssemblyContextControlItem assemblyContext = EditingContext.Items.GetValue<AssemblyContextControlItem>();
                if (assemblyContext != null)
                {
                    return assemblyContext.AllAssemblyNamesInContext;
                }
                return null;
            }
        }

    }
}
