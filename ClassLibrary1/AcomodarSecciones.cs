using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CustomExporterAdnMeshJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using Autodesk.Revit.ApplicationServices;
using Application = Autodesk.Revit.ApplicationServices.Application;
using static Autodesk.Revit.DB.SpecTypeId;
using Reference = Autodesk.Revit.DB.Reference;
using System.Collections.ObjectModel;

namespace RevitPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class AcomodarSecciones : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            //Esto nos permite trabajar con el proyecto de la vista activa, por si tenemos smmuchos abiertos ...
            Document doc = uidoc.Document;
           

                List<Element> lineas = new List<Element>();
                List<Reference> reflineas = new List<Reference>();
                List<Reference> refsecciones = new List<Reference>();
                List<string> nombredesecciones = new List<string>();
 

                //Este ciclo es para meter los elementos en listas ordenados por como se van seleccionando
                bool flag = true;
                while (flag)
                {
                    try
                    {
                        Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick elements in the desired order and hit ESC to stop picking.");
                        ElementId eleId = reference.ElementId;
                        Element ele = doc.GetElement(eleId);
                        Type tipo = ele.GetType();

                        if (ele is DetailLine)
                        {
                            lineas.Add(ele);
                            reflineas.Add(reference);
                        }

                       if (ele.Category.Name == "Views" || ele.Category.Id.ToString() == "-2000278" || ele.Category.Name == "Vistas")
                    {
                            refsecciones.Add(reference);
                            nombredesecciones.Add(ele.Name);
                        }
                    }
                    catch
                    {
                        flag = false;
                    }
                }

          using (Transaction tx = new Transaction(doc, "Sections2"))
          {
                tx.Start();
       
             int contador = 0;
             foreach (Element x in lineas)
             {

                    Reference lineas1 = reflineas[contador];
                    ElementId eleId1 = lineas1.ElementId;
                    Element ele1 = doc.GetElement(eleId1);


                    LocationCurve linea = ele1.Location as LocationCurve;
                    XYZ pt1 = linea.Curve.GetEndPoint(0);
                    XYZ pt2 = linea.Curve.GetEndPoint(1);


                    //Vector de la linea
                    XYZ VectorLinea = pt2 - pt1;
                    //Angulo de la linea con respecto al eje X
                    double angulodelinea = VectorLinea.AngleTo(XYZ.BasisX);

                    XYZ p1 = new XYZ(0, 0, 0);
                    XYZ p2 = new XYZ(0, 0, 1);

                    //Line axis = Line.CreateBound(p1, p2);

                    //Elemento Seccion
                    ElementId IdElementoSeccion = refsecciones[contador].ElementId;
                    Element ElementoSeccion = doc.GetElement(IdElementoSeccion);
                    Type Secciontipo = ElementoSeccion.GetType();
                    
                    
                    //Linea del corte
                    Line lineadeseccion = GetSectionLine1(ElementoSeccion);
                    XYZ Ptolineacorte1 = lineadeseccion.GetEndPoint(0);
                    XYZ Ptolineacorte2 = lineadeseccion.GetEndPoint(1);
                    XYZ VectorLineaSeccion = Ptolineacorte2 - Ptolineacorte1;
                    XYZ translationVec = VectorLinea - VectorLineaSeccion;


                    if (!translationVec.IsZeroLength())
                    {
                        ElementTransformUtils.MoveElement(doc, IdElementoSeccion, translationVec);
                    }

                    //oldnormal
                    XYZ oldNormal = VectorLineaSeccion.Normalize();
                    XYZ newNormal = VectorLinea.Normalize();

                    double angle = oldNormal.AngleTo(newNormal);

                    // Need to adjust the rotation angle based on the direction of rotation (not covered by AngleTo)
                    XYZ cross = oldNormal.CrossProduct(newNormal).Normalize();
                    double sign = 1.0;
                    if (!cross.IsAlmostEqualTo(XYZ.BasisZ))
                    {
                        sign = -1.0;
                    }
                    angle *= sign;


                    if (Math.Abs(angle) > 0)
                    {
                        Line axis1 = Line.CreateBound(VectorLinea, VectorLinea + XYZ.BasisZ);
                        ElementTransformUtils.RotateElement(doc, IdElementoSeccion, axis1, angle);
                    }

                    Line nlineadeseccion = GetSectionLine1(ElementoSeccion);
                    Ptolineacorte1 = nlineadeseccion.GetEndPoint(0);
                    Ptolineacorte2 = nlineadeseccion.GetEndPoint(1);

                    XYZ puntoMedioLinea = (pt1 + pt2) / 2.0;
                    XYZ puntoMedioCorte = (Ptolineacorte1 + Ptolineacorte2) / 2.0;
                    XYZ translationVec1 = puntoMedioLinea - puntoMedioCorte;

                    ElementTransformUtils.MoveElement(doc, IdElementoSeccion, translationVec1);

                //----------------------------------------------------------

                XYZ pt11 = linea.Curve.GetEndPoint(0);
                XYZ pt22 = linea.Curve.GetEndPoint(1);
                XYZ v = pt22 - pt11;

                double length = linea.Curve.Length;
                
                double altura = 500;
                
                double desfase = Configuracion.desfase;
                desfase = 0;

                double profundidad = 51000;

                double w1 = length / 2;
                XYZ max = new XYZ(w1, altura, profundidad);
                XYZ min = new XYZ(-w1, desfase, 0);

                XYZ midpoint = pt11 + 0.5 * v;
                XYZ dir = v.Normalize();
                XYZ up = XYZ.BasisZ;
                XYZ viewdir = dir.CrossProduct(up);
                Transform t = Transform.Identity;
                t.Origin = midpoint;
                t.BasisX = dir;
                t.BasisY = up;
                t.BasisZ = viewdir;

                BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
                sectionBox.Transform = t;
                sectionBox.Min = min;
                sectionBox.Max = max;

                //   --------------------------------------------

                ViewFamilyType vft
                   = new FilteredElementCollector(doc)
                     .OfClass(typeof(ViewFamilyType))
                     .Cast<ViewFamilyType>()
                     .FirstOrDefault<ViewFamilyType>(xs =>
                     ViewFamily.Section == xs.ViewFamily);


                 FilteredElementCollector COLLECTOR = new FilteredElementCollector(doc).OfClass(typeof(ViewSection));
                 ICollection<Element> Viewsections1 = COLLECTOR.ToElements() as ICollection<Element>;

                if (Viewsections1.Count() != 0)
                {
                    foreach (ViewSection vs in Viewsections1)
                    {
                            if (vs.Name == nombredesecciones[contador])
                            {
                                vs.CropBox = sectionBox;
                                break;
                            }
                    }
                }

              contador++;
             
             }
           tx.Commit();
          }
            uidoc.UpdateAllOpenViews();
            uidoc.RefreshActiveView();
            return Result.Succeeded;
        }


        //Este Metodo recibe una Section y devuelve una Linea sobre la misma
        Autodesk.Revit.DB.Line GetSectionLine1(Element section)
        {

            const double correction = 21.130014403 / 304.8;
            Document doc = section.Document;
            Category cat = section.Category;
            Autodesk.Revit.DB.View view = doc.ActiveView;

            Autodesk.Revit.DB.View viewFromSection = null;

            FilteredElementCollector views
                = new FilteredElementCollector(doc)
                  .OfClass(typeof(Autodesk.Revit.DB.View));

            foreach (Autodesk.Revit.DB.View v in views)
            {
                if (section.Name == v.Name
                  && section.GetTypeId() == v.GetTypeId())
                {
                    viewFromSection = v;
                    break;
                }
            }

            BoundingBoxXYZ bb = section.get_BoundingBox(view);
            XYZ pt1 = bb.Min;
            XYZ pt2 = bb.Max;

            XYZ Origin = viewFromSection.Origin;
            XYZ ViewBasisX = viewFromSection.RightDirection;
            XYZ ViewBasisY = viewFromSection.ViewDirection;
            if (ViewBasisX.X < 0 ^ ViewBasisX.Y < 0)
            {
                double d = pt1.Y;
                pt1 = new XYZ(pt1.X, pt2.Y, pt1.Z);
                pt2 = new XYZ(pt2.X, d, pt2.Z);
            }
            XYZ ToPlane1 = pt1.Add(ViewBasisY.Multiply(
              ViewBasisY.DotProduct(Origin.Subtract(pt1))));

            XYZ ToPlane2 = pt2.Subtract(ViewBasisY.Multiply(
              ViewBasisY.DotProduct(pt2.Subtract(Origin))));

            XYZ correctionVector = ToPlane2.Subtract(ToPlane1)
              .Normalize().Multiply(correction);

            XYZ endPoint0 = ToPlane1.Add(correctionVector);
            XYZ endPoint1 = ToPlane2.Subtract(correctionVector);

            return Autodesk.Revit.DB.Line.CreateBound(endPoint0, endPoint1);

        }

    }

}

