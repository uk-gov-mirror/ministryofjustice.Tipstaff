using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Tipstaff.Models;

namespace Tipstaff.Helpers
{
    public static class WordTableBuilder
    {

        public static IEnumerable<OpenXmlElement> BuildAddressTable(IEnumerable<string> addressLines, int widthDxa = 8856)
        {
            var lines = addressLines.Where(l => l != null).ToList();
            var para = new Paragraph(
                new ParagraphProperties(
                    new ParagraphMarkRunProperties(new Languages { Val = "EN-GB" })
                )
            );
            for (int i = 0; i < lines.Count; i++)
            {
                para.AppendChild(new Run(
                    new RunProperties(new Languages { Val = "EN-GB" }),
                    new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve }
                ));
                if (i < lines.Count - 1)
                    para.AppendChild(new Run(new Break()));
            }

            var table = new Table();
            table.AppendChild(StandardTableProps());
            table.AppendChild(new TableGrid(
                new GridColumn { Width = widthDxa.ToString() }
            ));
            table.AppendChild(Row(Cell(widthDxa.ToString(), para)));

            yield return table;
            yield return SpacerParagraph();
        }

        public static IEnumerable<OpenXmlElement> BuildChildTable(Child child, int childNumber)
        {
            var table = new Table();
            table.AppendChild(StandardTableProps());
            table.AppendChild(new TableGrid(
                new GridColumn { Width = "1715" },
                new GridColumn { Width = "1337" },
                new GridColumn { Width = "1268" },
                new GridColumn { Width = "1650" },
                new GridColumn { Width = "2886" }
            ));

            table.AppendChild(Row(
                SpanCell("8856", $"CHILD {childNumber}", gridSpan: 5, shaded: true, bold: true)
            ));
            table.AppendChild(Row(
                LabelCell("1715", "Surname"),
                Cell("2605", child.nameLast?.ToUpper() ?? string.Empty, gridSpan: 2),
                LabelCell("1650", "Forenames"),
                Cell("2886", $"{child.nameFirst} {child.nameMiddle}".Trim())
            ));
            table.AppendChild(Row(
                LabelCell("1715", "Date of Birth"),
                Cell("2605", child.DateofBirthDisplay, gridSpan: 2),
                LabelCell("1650", "Height"),
                Cell("2886", child.height ?? string.Empty)
            ));
            table.AppendChild(Row(
                LabelCell("1715", "Age"),
                Cell("2605", child.Age, gridSpan: 2),
                LabelCell("1650", "Build"),
                Cell("2886", child.build ?? string.Empty)
            ));
            table.AppendChild(Row(
                LabelCell("1715", "Sex"),
                Cell("2605", child.gender?.detail ?? string.Empty, gridSpan: 2),
                LabelCell("1650", "Hair colour"),
                Cell("2886", child.hairColour ?? string.Empty)
            ));
            table.AppendChild(Row(
                LabelCell("1715", "Nationality"),
                Cell("2605", child.country?.Detail ?? string.Empty, gridSpan: 2),
                LabelCell("1650", "Eye colour"),
                Cell("2886", child.eyeColour ?? string.Empty)
            ));
            table.AppendChild(Row(
                LabelCell("1715", string.Empty),
                Cell("2605", string.Empty, gridSpan: 2),
                LabelCell("1650", "Skin colour"),
                Cell("2886", child.SkinColour?.Detail ?? string.Empty)
            ));
            table.AppendChild(Row(
                LabelCell("3052", "Special features", gridSpan: 2),
                Cell("5804", child.specialfeatures ?? string.Empty, gridSpan: 3)
            ));

            yield return table;
            yield return SpacerParagraph();
        }

        public static IEnumerable<OpenXmlElement> BuildRespondentTables(Respondent resp)
        {
            yield return BuildRespondentDetailsTable(resp);
            yield return BuildKnownRisksTable(resp);
            yield return SpacerParagraph();
        }

        // ---------------------------------------------------------------
        // Respondent tables
        // ---------------------------------------------------------------

        private static Table BuildRespondentDetailsTable(Respondent resp)
        {
            var table = new Table();
            table.AppendChild(StandardTableProps());
            table.AppendChild(new TableGrid(
                new GridColumn { Width = "2448" },
                new GridColumn { Width = "2520" },
                new GridColumn { Width = "1674" },
                new GridColumn { Width = "2214" }
            ));

            table.AppendChild(Row(
                LabelCell("2448", "Name"),
                Cell("6408", resp.PoliceDisplayName, gridSpan: 3)
            ));
            table.AppendChild(Row(
                LabelCell("2448", "Relationship to child"),
                Cell("2520", resp.childRelationship?.Detail ?? string.Empty),
                LabelCell("1674", "Date of Birth"),
                Cell("2214", resp.DateofBirthDisplay)
            ));
            table.AppendChild(Row(
                LabelCell("2448", "Age"),
                Cell("2520", resp.Age),
                LabelCell("1674", "Hair colour"),
                Cell("2214", resp.hairColour ?? string.Empty)
            ));
            table.AppendChild(Row(
                LabelCell("2448", "Eye Colour"),
                Cell("2520", resp.eyeColour ?? string.Empty),
                LabelCell("1674", "Skin colour"),
                Cell("2214", resp.SkinColour?.Detail ?? string.Empty)
            ));
            table.AppendChild(Row(
                LabelCell("2448", "Height"),
                Cell("2520", resp.height ?? string.Empty),
                LabelCell("1674", "Build"),
                Cell("2214", resp.build ?? string.Empty)
            ));
            table.AppendChild(Row(
                LabelCell("2448", "Nationality"),
                Cell("6408", resp.country?.Detail ?? string.Empty, gridSpan: 3)
            ));
            table.AppendChild(Row(
                LabelCell("2448", "Special features"),
                Cell("6408", resp.specialfeatures ?? string.Empty, gridSpan: 3)
            ));

            return table;
        }

        private static Table BuildKnownRisksTable(Respondent resp)
        {
            var table = new Table();
            table.AppendChild(StandardTableProps());
            table.AppendChild(new TableGrid(
                new GridColumn { Width = "2448" },
                new GridColumn { Width = "2520" },
                new GridColumn { Width = "1674" },
                new GridColumn { Width = "2214" }
            ));

            table.AppendChild(Row(
                SpanCell("8856", "Known Risks", gridSpan: 4, shaded: true)
            ));
            table.AppendChild(Row(
                LabelCell("2448", "Violence"),
                Cell("6408", resp.riskOfViolence ?? string.Empty, gridSpan: 3)
            ));
            table.AppendChild(Row(
                LabelCell("2448", "Drugs"),
                Cell("6408", resp.riskOfDrugs ?? string.Empty, gridSpan: 3)
            ));

            return table;
        }

        private static TableProperties StandardTableProps()
        {
            return new TableProperties(
                new TableStyle { Val = "TableGrid" },
                new TableWidth { Width = "0", Type = TableWidthUnitValues.Auto },
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                ),
                new TableLook { Val = "01E0" }
            );
        }

        // Empty paragraph inserted after each table for visual spacing
        private static Paragraph SpacerParagraph()
        {
            return new Paragraph(
                new ParagraphProperties(
                    new ParagraphMarkRunProperties(new Languages { Val = "EN-GB" })
                )
            );
        }

        private static TableRow Row(params TableCell[] cells)
        {
            var row = new TableRow();
            foreach (var cell in cells)
                row.AppendChild(cell);
            return row;
        }

        private static TableCell Cell(string width, string text,
            int gridSpan = 1, bool shaded = false)
        {
            var props = new TableCellProperties(
                new TableCellWidth { Width = width, Type = TableWidthUnitValues.Dxa }
            );
            if (gridSpan > 1) props.AppendChild(new GridSpan { Val = gridSpan });
            if (shaded) props.AppendChild(new Shading
                { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "D9D9D9" });

            return new TableCell(props,
                new Paragraph(new Run(
                    new Text(text) { Space = SpaceProcessingModeValues.Preserve }
                ))
            );
        }

        private static TableCell Cell(string width, Paragraph para, int gridSpan = 1)
        {
            var props = new TableCellProperties(
                new TableCellWidth { Width = width, Type = TableWidthUnitValues.Dxa }
            );
            if (gridSpan > 1) props.AppendChild(new GridSpan { Val = gridSpan });
            return new TableCell(props, para);
        }

        private static TableCell LabelCell(string width, string text, int gridSpan = 1)
        {
            return Cell(width, text, gridSpan: gridSpan, shaded: true);
        }

        private static TableCell SpanCell(string width, string text,
            int gridSpan = 1, bool shaded = false, bool bold = false)
        {
            var props = new TableCellProperties(
                new TableCellWidth { Width = width, Type = TableWidthUnitValues.Dxa }
            );
            if (gridSpan > 1) props.AppendChild(new GridSpan { Val = gridSpan });
            if (shaded) props.AppendChild(new Shading
                { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "D9D9D9" });

            var runProps = bold ? new RunProperties(new Bold()) : null;
            var run = runProps != null
                ? new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve })
                : new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

            return new TableCell(props, new Paragraph(run));
        }
    }
}
