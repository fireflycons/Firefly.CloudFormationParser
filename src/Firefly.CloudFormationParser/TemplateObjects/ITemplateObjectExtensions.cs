namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System.Collections.Generic;

    using Firefly.CloudFormationParser.Intrinsics;
    using Firefly.CloudFormationParser.Intrinsics.Abstractions;
    using Firefly.CloudFormationParser.Intrinsics.Functions;
    using Firefly.CloudFormationParser.Utils;

    /// <summary>
    /// Extension methods for <see cref="ITemplateObject"/>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal static class ITemplateObjectExtensions
    {
        public static void Visit(
            this ITemplateObject self,
            IDictionary<string, object> start,
            ITemplateObjectVisitor templateObjectVisitor)
        {
            // Walk resource properties
            PropertyPath path = new PropertyPath();

            WalkDict(start);

            void WalkObject(object obj)
            {
                switch (obj)
                {
                    case Dictionary<string, object> dict1:

                        templateObjectVisitor.BeforeVisitObject(self, path, dict1);
                        WalkDict(dict1);
                        templateObjectVisitor.AfterVisitObject(self, path, dict1);
                        break;

                    case Dictionary<object, object> dict2:

                        templateObjectVisitor.BeforeVisitObject(self, path, dict2);
                        WalkDict(dict2);
                        templateObjectVisitor.AfterVisitObject(self, path, dict2);
                        break;

                    case List<object> list:

                        templateObjectVisitor.BeforeVisitList(self, path, list);
                        WalkList(list);
                        templateObjectVisitor.AfterVisitList(self, path, list);
                        break;

                    case List<string> list:

                        templateObjectVisitor.BeforeVisitList(self, path, list);
                        WalkList(list);
                        templateObjectVisitor.AfterVisitList(self, path, list);
                        break;

                    case IIntrinsic intrinsic:

                        ProcessIntrinsic(intrinsic);
                        break;
                }
            }

            void WalkDict<T>(IDictionary<T, object> dict)
            {
                foreach (var kvp in dict)
                {
                    path.Push(kvp.Key!.ToString());

                    switch (kvp.Value)
                    {
                        case IIntrinsic intrinsic:

                            ProcessIntrinsic(intrinsic);
                            break;

                        case Dictionary<string, object> dict1:

                            templateObjectVisitor.BeforeVisitObject(self, path, dict1);
                            WalkDict(dict1);
                            templateObjectVisitor.AfterVisitObject(self, path, dict1);
                            break;

                        case Dictionary<object, object> dict2:

                            templateObjectVisitor.BeforeVisitObject(self, path, dict2);
                            WalkDict(dict2);
                            templateObjectVisitor.AfterVisitObject(self, path, dict2);
                            break;

                        case List<object> list:

                            templateObjectVisitor.BeforeVisitList(self, path, list);
                            WalkList(list);
                            templateObjectVisitor.AfterVisitList(self, path, list);
                            break;

                        case List<string> list:

                            templateObjectVisitor.BeforeVisitList(self, path, list);
                            WalkList(list);
                            templateObjectVisitor.AfterVisitList(self, path, list);
                            break;

                        default:

                            templateObjectVisitor.VisitProperty(self, path, kvp);
                            break;
                    }

                    path.Pop();
                }
            }

            void WalkList<T>(IEnumerable<T> list)
            {
                foreach (var (item, index) in list.WithIndex())
                {
                    path.Push(index.ToString());

                    switch (item)
                    {
                        case null:

                            break;

                        case IIntrinsic intrinsic:

                            ProcessIntrinsic(intrinsic);
                            break;

                        case Dictionary<string, object> dict1:

                            templateObjectVisitor.BeforeVisitObject(self, path, dict1);
                            WalkDict(dict1);
                            templateObjectVisitor.AfterVisitObject(self, path, dict1);
                            break;

                        case Dictionary<object, object> dict2:

                            templateObjectVisitor.BeforeVisitObject(self, path, dict2);
                            WalkDict(dict2);
                            templateObjectVisitor.AfterVisitObject(self, path, dict2);
                            break;

                        case List<object> list1:

                            templateObjectVisitor.BeforeVisitList(self, path, list1);
                            WalkList(list1);
                            templateObjectVisitor.AfterVisitList(self, path, list1);
                            break;

                        case List<string> list1:

                            templateObjectVisitor.BeforeVisitList(self, path, list1);
                            WalkList(list1);
                            templateObjectVisitor.AfterVisitList(self, path, list1);
                            break;

                        default:

                            templateObjectVisitor.VisitListItem(self, path, item);
                            break;
                    }

                    path.Pop();
                }
            }

            void ProcessIntrinsic(IIntrinsic intrinsic)
            {
                path.Push(intrinsic.TagName);

                if (templateObjectVisitor.VisitIntrinsic(self, path, intrinsic))
                {
                    switch (intrinsic)
                    {
                        case IBranchableIntrinsic branchableIntrinsic:
                            {
                                WalkObject(branchableIntrinsic.GetBranch(self.Template));
                                break;
                            }

                        case GetAttIntrinsic { AttributeName: IIntrinsic _ } getAttIntrinsic:

                            WalkObject(getAttIntrinsic.AttributeName);
                            break;

                        case SubIntrinsic subIntrinsic:

                            WalkDict(subIntrinsic.Substitutions);
                            WalkList(subIntrinsic.ImplicitReferences);
                            break;

                        case AbstractArrayIntrinsic arrayIntrinsic:

                            WalkList(arrayIntrinsic.Items);
                            break;
                    }
                }

                path.Pop();
            }
        }
    }
}