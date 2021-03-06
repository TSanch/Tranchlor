Module Meteo
    Dim arrPanne(440000) As StrctPanne 'matrice d'analyse des pannes, con�u pour 50ans mesure chaque heure
    Dim arrMatrice(440000) As StrctCalc 'matrice de calcul, con�u pour 50ans mesure chaque heure
    Dim arrDaten(440000) As StrctMeteo 'matrice input m�t�o, con�u pour 50ans mesure chaque heure
    Dim frmTempSeuil As New TempSeuil
    Dim iAnzahl As Integer 'nombre de ligne
    Dim CasInput As Integer '[-]

    Structure StrctMeteo 'colonnes de la matrice � partir du fichier METEO_*.txt
        Public datum As Integer 'date YYYYMMDD
        Public heure As Integer 'heure HH
        Public moy6 As Single 'temp�rature [0.1�C]
        Public moy13 As Single 'humidit� relative [0/00]
        Public moy17 As Long 'h pluie [0.1mm]
        Public moy22 As Single 'rayonnement globale [Wh/m2]
        Public moy80 As Single 'h neige neuve [mm]
        Public neige As Single  'h neige calcul�
    End Structure

    Structure StrctCalc 'colonnes de la matrice de calcul
        Public year1 As Integer 'ann�e YYYY
        Public month As Integer 'mois MM
        Public day As Integer 'jour DD
        Public hour As Integer 'heure HH
        Public year2 As Single 'ann�e en d�cimale YYYY,....
        Public HR_brouillard As Single 'exposition brouillard [%]
        Public HR_eclaboussures As Single 'exposition eclaboussures [%]
        Public HR_direct As Single 'exposition directe [%]
        Public HR_ext As Single 'exposition � l'ext�rieur � l'abri des pr�cipitations [%]
        Public HR_caisson As Single 'exposition dans les caissons [%]
        Public salage1 As String 'salage m�canique
        Public salage2 As String 'salage automatique
        Public T As Single 'temp�rature air ventil�e [�C]
        Public Ts As Single ' temp�rature de surface �quivalente [�C]
        Public Tcaisson As Single   'temp�rature � l'int�rieur caisson [�C]
        Public Text As Single   'temp�rature ext�rieure, � l'abri des pr�cipitations [�C]
    End Structure

    Structure StrctPanne 'colonnes de la matrice des pannes
        Public PanneStart As Integer 'colonnes d�but des pannes
        Public PanneEnd As Integer 'colonnes fin des pannes
        Public PanneMesure As String ' colonnes des types de pannes
    End Structure

    Structure File 'fichier INPUT
        Public HR As Single 'colonnes HR
        Public Sel As Single 'colonnes salage
        Public Tsurf As Single ' colonnes Temp�rature de surface (T ou Ts)
    End Structure

    Public Sub MeteoTreatment()
        Dim NbrAns As Integer '[-]
        Dim i As Integer '[-]
        Dim txtFile As String
        Dim posTxt As Integer
        Dim nFic As Integer = Microsoft.VisualBasic.FileSystem.FreeFile()
        Dim OutFile As String
        Dim Filtre As String
        Dim Index As Short
        Dim Directoire As Boolean
        Dim Titre As String
        Dim Canc As Boolean = False
        Dim line1 As String
        Dim line2 As String
        Dim line3 As String
        Dim bFertig As Boolean

        Dim MyPos6 As Integer
        Dim MyPos13 As Integer
        Dim MyPos17 As Integer
        Dim MyPos22 As Integer
        Dim MyPos80 As Integer
        Dim PostFile As String
        Dim InputMatrice(440000) As Single
        Dim OutputMatrice(440000) As Single

        Dim Hiv As Boolean = True
        Dim Cpt As Short = 0
        Dim NDH As Single = 0
        Dim qNaCl1 As Single = 0
        Dim qNaCl2 As Single = 0
        Dim PluieOld As Boolean = False
        Dim QNa1 As Single = 0
        Dim QNa2 As Single = 0
        Dim EpNa1 As Single = 0
        Dim EpNa2 As Single = 0
        Dim Dint1 As Short
        Dim Dint2 As Short
        Dim DureeInt As Short
        Dim HRseuil As Single = 0
        Dim Tseuil1 As Single = 0
        Dim Tseuil2 As Single = 0
        Dim Feau As Single
        ReDim arrPanne(440000)
        ReDim arrMatrice(440000)
        ReDim arrDaten(440000)

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'lecture fichier METEO_*.txt
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Filtre = "Text files (METEO_*.txt)|METEO_*.txt"
        Index = 1
        Directoire = True
        Titre = "S�lectionner le fichier m�t�o"
        OpenDialog(OutFile, Canc, Filtre, Index, Directoire, Titre)
        If Canc = True Then GoTo b

        FileOpen(nFic, OutFile, OpenMode.Input, OpenAccess.Read, OpenShare.Shared)
        FilePost(OutFile, PostFile)
        FileOnly(OutFile)
        posTxt = Len(OutFile) - 10
        txtFile = Mid(OutFile, 7, posTxt)
        Input(nFic, line1) 'lire ligne 1 et 2 de METEO_*.txt et faire rien
        Input(nFic, line2)
        Input(nFic, line3) 'lire linge 3

        MyPos6 = InStr(1, line3, "6") 'recherche des titre des colonnes 
        MyPos13 = InStr(1, line3, "13")
        MyPos17 = InStr(1, line3, "17")
        MyPos22 = InStr(1, line3, "22")
        MyPos80 = InStr(1, line3, "80")

        If MyPos80 <> 0 Then
            CasInput = 1 'matriceStrctMeteo avec les colonnes 6,13,17,22,80
        End If
        If MyPos80 = 0 Then
            CasInput = 2 'matriceStrctMeteo avec les colonnes 6,13,17,22 (sans neige)
        End If

        bFertig = False
        i = 0
        Do While Not bFertig
            Try 'test s'il y a du text ou pas
                Input(nFic, arrDaten(i).datum)
            Catch
                bFertig = True
            End Try
            If Not bFertig Then
                ' les quatre cas:
                If CasInput = 1 Then
                    Input(nFic, arrDaten(i).heure)
                    Input(nFic, arrDaten(i).moy6)
                    Input(nFic, arrDaten(i).moy13)
                    Input(nFic, arrDaten(i).moy17)
                    Input(nFic, arrDaten(i).moy22)
                    Input(nFic, arrDaten(i).moy80)
                ElseIf CasInput = 2 Then 'sans arrDaten(i).moy80
                    Input(nFic, arrDaten(i).heure)
                    Input(nFic, arrDaten(i).moy6)
                    Input(nFic, arrDaten(i).moy13)
                    Input(nFic, arrDaten(i).moy17)
                    Input(nFic, arrDaten(i).moy22)
                End If
                If (arrDaten(i).datum - ((Fix(arrDaten(i).datum / 10000)) * 10000) <> 229) Then i = i + 1 '�limination du 29. f�vrier
            End If 'If Not bFertig
        Loop
        iAnzahl = i
        FileClose(nFic)

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'calcul des dates dans la matrice
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        For i = 0 To (iAnzahl - 1)
            arrMatrice(i).year1 = Fix(arrDaten(i).datum / 10000)
            arrMatrice(i).month = Fix((arrDaten(i).datum - 10000 * arrMatrice(i).year1) / 100)
            arrMatrice(i).day = arrDaten(i).datum - arrMatrice(i).year1 * 10000 - arrMatrice(i).month * 100
            arrMatrice(i).hour = arrDaten(i).heure
            arrMatrice(i).year2 = arrMatrice(i).year1 + arrMatrice(i).month / 12 + arrMatrice(i).day / 365 + arrMatrice(i).hour / (24 * 365)
        Next

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'd�tection des pannes
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim Panne As Boolean
        Dim NbrPanne As Integer
        Panne = False '= pas de panne
        NbrPanne = 0

        For i = 0 To (iAnzahl - 1)
            If arrDaten(i).moy6 = 32767 And Not Panne Then
                Panne = True
                arrPanne(NbrPanne).PanneStart = i
                arrPanne(NbrPanne).PanneMesure = "mm de pluie"
            End If
            If i = iAnzahl - 1 And Panne = True Then
                Panne = False
                arrPanne(NbrPanne).PanneEnd = i
                NbrPanne = NbrPanne + 1
            End If
            If arrDaten(i).moy6 <> 32767 And Panne Then
                Panne = False
                arrPanne(NbrPanne).PanneEnd = i - 1
                NbrPanne = NbrPanne + 1
            End If
        Next i
        For i = 0 To (iAnzahl - 1)
            If arrDaten(i).moy13 = 32767 And Not Panne Then
                Panne = True
                arrPanne(NbrPanne).PanneStart = i
                arrPanne(NbrPanne).PanneMesure = "Temp�rature"
            End If
            If i = iAnzahl - 1 And Panne = True Then
                Panne = False
                arrPanne(NbrPanne).PanneEnd = i
                NbrPanne = NbrPanne + 1
            End If
            If arrDaten(i).moy13 <> 32767 And Panne Then
                Panne = False
                arrPanne(NbrPanne).PanneEnd = i - 1
                NbrPanne = NbrPanne + 1
            End If
        Next i
        For i = 0 To (iAnzahl - 1)
            If arrDaten(i).moy17 = 32767 And Not Panne Then
                Panne = True
                arrPanne(NbrPanne).PanneStart = i
                arrPanne(NbrPanne).PanneMesure = "Humidit� relaltive"
            End If
            If i = iAnzahl - 1 And Panne = True Then
                Panne = False
                arrPanne(NbrPanne).PanneEnd = i
                NbrPanne = NbrPanne + 1
            End If
            If arrDaten(i).moy17 <> 32767 And Panne Then
                Panne = False
                arrPanne(NbrPanne).PanneEnd = i - 1
                NbrPanne = NbrPanne + 1
            End If
        Next i
        For i = 0 To (iAnzahl - 1)
            If arrDaten(i).moy22 = 32767 And Not Panne Then
                Panne = True
                arrPanne(NbrPanne).PanneStart = i
                arrPanne(NbrPanne).PanneMesure = "Rayonnement globale"
            End If
            If i = iAnzahl - 1 And Panne = True Then
                Panne = False
                arrPanne(NbrPanne).PanneEnd = i
                NbrPanne = NbrPanne + 1
            End If
            If arrDaten(i).moy22 <> 32767 And Panne Then
                Panne = False
                arrPanne(NbrPanne).PanneEnd = i - 1
                NbrPanne = NbrPanne + 1
            End If
        Next i

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'afficher les pannes
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim MessagePanne As String
        Dim strDebut As String
        Dim strFin As String
        For i = 0 To NbrPanne - 1
            strDebut = arrMatrice(arrPanne(i).PanneStart).day & "." & arrMatrice(arrPanne(i).PanneStart).month & "." & arrMatrice(arrPanne(i).PanneStart).year1 & " � " & arrMatrice(arrPanne(i).PanneStart).hour & "H  "
            strFin = arrMatrice(arrPanne(i).PanneEnd).day & ". " & arrMatrice(arrPanne(i).PanneEnd).month & "." & arrMatrice(arrPanne(i).PanneEnd).year1 & " � " & arrMatrice(arrPanne(i).PanneEnd).hour & "H  "
            MessagePanne = MessagePanne + "Panne " + CStr(i + 1) + "  du  " + strDebut + "  au  " + strFin + arrPanne(i).PanneMesure + (vbCrLf)
        Next
        If NbrPanne = 0 Then
            MessagePanne = "Il n'y a pas de panne dans la s�rie!"
        End If
        MsgBox(MessagePanne, , "D�t�ction des pannes")

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'recherche de l'intervalle le plus long sans pannes
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim Start As Integer
        Dim Fin As Integer
        Dim startmax As Integer
        Dim finmax As Integer
        Dim IntLong As Integer
        Dim IntrStart As Integer

        Start = 0
        Fin = 0
        startmax = 0
        finmax = 0
        Panne = False
        For i = 0 To iAnzahl - 1 'i correspond � une heure
            If arrDaten(i).moy6 = 32767 Or arrDaten(i).moy13 = 32767 Or arrDaten(i).moy17 = 32767 Or arrDaten(i).moy22 = 32767 Then
                If Panne = False Then
                    Fin = i - 1
                    If Fin - Start > finmax - startmax Then
                        finmax = Fin
                        startmax = Start
                    End If
                    Panne = True
                End If
            Else
                If Panne = True Then
                    Start = i
                    Panne = False
                End If
            End If
        Next

        If Panne = False Then 'contr�le du dernier intervalle
            Fin = i - 1
            If Fin - Start > finmax - startmax Then
                finmax = Fin
                startmax = Start
            End If
        End If

        IntLong = finmax - startmax
        IntLong = Fix(IntLong / 8760)
        iAnzahl = 8760 * IntLong

        strDebut = arrMatrice(startmax).day & "." & arrMatrice(startmax).month & "." & arrMatrice(startmax).year1 & " � " & arrMatrice(startmax).hour & "H  "
        strFin = arrMatrice(startmax + iAnzahl - 1).day & "." & arrMatrice(startmax + iAnzahl - 1).month & "." & arrMatrice(startmax + iAnzahl - 1).year1 & " � " & arrMatrice(startmax + iAnzahl - 1).hour & "H  "
        MsgBox(" du  " & strDebut & " au  " & strFin, , "Interval maximal sans pannes ")

        NbrAns = iAnzahl / 8760

        For i = 0 To iAnzahl - 1
            arrMatrice(i) = arrMatrice(i + startmax)
        Next
        ReDim Preserve arrMatrice(iAnzahl - 1)
        For i = 0 To iAnzahl - 1
            arrDaten(i) = arrDaten(i + startmax)
        Next
        ReDim Preserve arrDaten(iAnzahl - 1)

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'Calcul du nbre d'interventions et de la quantit� de sel �pandu
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        'Calcul du nombre de jours hivernaux
        For i = 0 To iAnzahl - 1
            If arrDaten(i).moy6 / 10 > 0 Then Hiv = False
            If Cpt = 24 Then
                If Hiv = True Then NDH = NDH + 1
                Hiv = True
                Cpt = 0
            End If
            Cpt = Cpt + 1
        Next
        NDH = NDH / NbrAns  'nombre de jours hivernaux par ans
        qNaCl1 = 20.83519974 * NDH + 211.3117439   'quantit� par an en g/m2 de sel d�vers� sur la chauss�e
        qNaCl2 = 20.83519974 * NDH - 72.9892168  'quantit� par an en g/m2 de sel d�vers� sur la chauss�e
        frmTempSeuil.Label12.Text = NbrAns
        frmTempSeuil.Label3.Text = CInt(qNaCl1)
        frmTempSeuil.Label74.Text = CInt(qNaCl2)
        frmTempSeuil.NumericUpDown1.Text = 10

        frmTempSeuil.Button2.Hide()
        frmTempSeuil.ShowDialog()
        frmTempSeuil.Hide()

        'calcul de la concentration en NaCl dans l'eau
        DureeInt = frmTempSeuil.NumericUpDown2.Text
        QNa1 = frmTempSeuil.NumericUpDown1.Text
        QNa2 = frmTempSeuil.NumericUpDown24.Text * frmTempSeuil.NumericUpDown25.Text
        Tseuil1 = frmTempSeuil.Label22.Text
        Tseuil2 = frmTempSeuil.Label66.Text
        HRseuil = frmTempSeuil.NumericUpDown3.Text
        EpNa1 = frmTempSeuil.NumericUpDown4.Text / 100
        EpNa2 = frmTempSeuil.NumericUpDown23.Text / 100
        Feau = frmTempSeuil.NumericUpDown5.Text
        Dint1 = 0
        Dint2 = 0
        PluieOld = False
        For i = 0 To iAnzahl - 1
            If Dint1 <> 0 Then Dint1 = Dint1 + 1
            If Dint2 <> 0 Then Dint2 = Dint2 + 1
            If Dint1 >= DureeInt Then Dint1 = 0
            If Dint1 = 0 And arrDaten(i).moy6 / 10 < Tseuil1 And (arrDaten(i).moy13 / 10 >= HRseuil Or arrDaten(i).moy17 / 10 > 0) Then
                If arrDaten(i).moy17 / 10 = 0 Then
                    arrMatrice(i).salage1 = EpNa1
                Else
                    If PluieOld = False Then
                        arrMatrice(i).salage1 = QNa1 / (1000 * arrDaten(i).moy17 / 10)
                    Else
                        If i <> 0 Then arrMatrice(i).salage1 = (arrMatrice(i - 1).salage1 * 1000 * Feau + QNa1) / ((Feau + arrDaten(i).moy17 / 10) * 1000)
                    End If
                End If
                Dint1 = Dint1 + 1
            End If
            If Dint2 = 0 And arrDaten(i).moy6 / 10 < Tseuil2 And (arrDaten(i).moy13 / 10 >= HRseuil Or arrDaten(i).moy17 / 10 > 0) Then
                If arrDaten(i).moy17 / 10 = 0 Then
                    arrMatrice(i).salage2 = EpNa2
                Else
                    If PluieOld = False Then
                        arrMatrice(i).salage2 = QNa2 / (1000 * arrDaten(i).moy17 / 10)
                    Else
                        If i <> 0 Then arrMatrice(i).salage2 = (arrMatrice(i - 1).salage2 * 1000 * Feau + QNa2) / ((Feau + arrDaten(i).moy17 / 10) * 1000)
                    End If
                End If
                Dint2 = Dint2 + 1
            End If
            If arrDaten(i).moy17 / 10 <> 0 Then
                PluieOld = True
            Else
                PluieOld = False
            End If
            If Dint1 <> 1 Then
                If PluieOld = True Then
                    If i <> 0 Then arrMatrice(i).salage1 = arrMatrice(i - 1).salage1 * 1000 * Feau / ((Feau + arrDaten(i).moy17 / 10) * 1000)
                Else
                    If i <> 0 Then arrMatrice(i).salage1 = arrMatrice(i - 1).salage1
                End If
                If i = 0 Then arrMatrice(i).salage1 = frmTempSeuil.NumericUpDown6.Value / 100
            End If
            If Dint2 <> 1 Then
                If PluieOld = True Then
                    If i > 0 Then arrMatrice(i).salage2 = arrMatrice(i - 1).salage2 * 1000 * Feau / ((Feau + arrDaten(i).moy17 / 10) * 1000)
                Else
                    If i > 0 Then arrMatrice(i).salage2 = arrMatrice(i - 1).salage2
                End If
                If i = 0 Then arrMatrice(i).salage2 = frmTempSeuil.NumericUpDown24.Value * frmTempSeuil.NumericUpDown25.Text / 100
                If arrMatrice(i).salage2 <= 0.1 * EpNa2 Then Dint2 = 0
            End If
            If arrMatrice(i).salage1 > EpNa1 Then arrMatrice(i).salage1 = EpNa1
            If arrMatrice(i).salage2 > EpNa2 Then arrMatrice(i).salage2 = EpNa2
        Next
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'calcul  T et Ts
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim a As Single
        Dim hy As Single
        a = 0.7
        hy = 20
        For i = 0 To iAnzahl - 1
            arrMatrice(i).T = arrDaten(i).moy6 / 10
            If arrDaten(i).moy22 < 0 Then
                arrDaten(i).moy22 = 0
            End If
            arrMatrice(i).Ts = arrMatrice(i).T + (a / hy) * arrDaten(i).moy22
        Next

        For i = 0 To iAnzahl - 1    'calcul de Text
            InputMatrice(i) = arrMatrice(i).T
        Next
        AttenBruit(CSng(frmTempSeuil.NumericUpDown8.Value), CSng(frmTempSeuil.NumericUpDown7.Value), CSng(frmTempSeuil.NumericUpDown9.Value), CSng(frmTempSeuil.NumericUpDown10.Value), InputMatrice, OutputMatrice, CSng(frmTempSeuil.TextBox1.Text))
        For i = 0 To iAnzahl - 1
            arrMatrice(i).Text = OutputMatrice(i)
        Next
        arrMatrice(iAnzahl - 1).Text = InputMatrice(iAnzahl - 1)

        For i = 0 To iAnzahl - 1    'calcul de Tcaisson
            InputMatrice(i) = arrMatrice(i).T
        Next
        AttenBruit(CSng(frmTempSeuil.NumericUpDown21.Value), CSng(frmTempSeuil.NumericUpDown22.Value), CSng(frmTempSeuil.NumericUpDown19.Value), CSng(frmTempSeuil.NumericUpDown20.Value), InputMatrice, OutputMatrice, CSng(frmTempSeuil.TextBox4.Text))
        For i = 0 To iAnzahl - 1
            arrMatrice(i).Tcaisson = OutputMatrice(i)
        Next
        arrMatrice(iAnzahl - 1).Tcaisson = InputMatrice(iAnzahl - 1)

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'calculs d'exposition HR
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        For i = 0 To (iAnzahl - 1)
            If arrDaten(i).moy13 >= 1000 Then 'exposition brouillard, pas de pluie
                arrMatrice(i).HR_brouillard = 99.99
            Else
                arrMatrice(i).HR_brouillard = arrDaten(i).moy13 / 10
            End If

            If i > 0 Then 'exposition eclaboussures
                If arrDaten(i).moy17 <> 0 And arrDaten(i - 1).moy17 <> 0 Then 'pluie avant une heure
                    arrMatrice(i).HR_eclaboussures = 100
                Else
                    If arrDaten(i).moy13 >= 1000 Then 'pas de pluie
                        arrMatrice(i).HR_eclaboussures = 99.99
                    Else
                        arrMatrice(i).HR_eclaboussures = arrDaten(i).moy13 / 10
                    End If
                End If
            End If

            If arrMatrice(i).hour > 17 Or arrMatrice(i).hour < 6 Then 'exposition stagnation (direct)
                'pendant la nuit (de 18hoo � 6h00)
                If arrDaten(i).moy17 <> 0 Then 'pluie
                    arrMatrice(i).HR_direct = 100
                Else
                    If arrDaten(i).moy13 >= 1000 Then 'pas de pluie
                        arrMatrice(i).HR_direct = 99.99
                    Else
                        arrMatrice(i).HR_direct = arrDaten(i).moy13 / 10
                    End If
                End If
            Else
                If arrDaten(i).moy17 <> 0 Then 'pluie
                    arrMatrice(i).HR_direct = 100
                Else
                    If arrDaten(i).moy13 >= 1000 Then 'pas de pluie
                        arrMatrice(i).HR_direct = 99.99
                    Else
                        arrMatrice(i).HR_direct = arrDaten(i).moy13 / 10
                    End If
                End If
            End If
        Next

        For i = 0 To iAnzahl - 1    'calcul de HRext
            InputMatrice(i) = arrMatrice(i).HR_brouillard
        Next
        AttenBruit(CSng(frmTempSeuil.NumericUpDown13.Value), CSng(frmTempSeuil.NumericUpDown14.Value), CSng(frmTempSeuil.NumericUpDown11.Value), CSng(frmTempSeuil.NumericUpDown12.Value), InputMatrice, OutputMatrice, CSng(frmTempSeuil.TextBox2.Text))
        For i = 0 To iAnzahl - 1
            arrMatrice(i).HR_ext = OutputMatrice(i)
        Next
        arrMatrice(iAnzahl - 1).HR_ext = InputMatrice(iAnzahl - 1)

        For i = 0 To iAnzahl - 1    'calcul de HRcaisson
            InputMatrice(i) = arrMatrice(i).HR_brouillard
        Next
        AttenBruit(CSng(frmTempSeuil.NumericUpDown17.Value), CSng(frmTempSeuil.NumericUpDown18.Value), CSng(frmTempSeuil.NumericUpDown15.Value), CSng(frmTempSeuil.NumericUpDown16.Value), InputMatrice, OutputMatrice, CSng(frmTempSeuil.TextBox3.Text))
        For i = 0 To iAnzahl - 1
            arrMatrice(i).HR_caisson = OutputMatrice(i)
        Next
        arrMatrice(iAnzahl - 1).HR_caisson = InputMatrice(iAnzahl - 1)

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'cr�ation des fichiers INPUT
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim INFile1, INFile2, INFile3, INFile4, INFile5, INFile6, INFile7, INFile8, INFile9, INFile10, INFile11, INFile12, INFile13, INFile14, INFile15, INFile16, INFile17 As System.IO.TextWriter
        OutFile = PostFile & "EXPO_M_E_E_" & txtFile & ".txt"
        INFile1 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_M_E_O_" & txtFile & ".txt"
        INFile2 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_M_B_E_" & txtFile & ".txt"
        INFile3 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_M_B_O_" & txtFile & ".txt"
        INFile4 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_M_D_E_" & txtFile & ".txt"
        INFile5 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_M_D_O_" & txtFile & ".txt"
        INFile6 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_M_EXT_" & txtFile & ".txt"
        INFile7 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_M_CAI_" & txtFile & ".txt"
        INFile8 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_M_CAC_" & txtFile & ".txt"
        INFile9 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_A_E_E_" & txtFile & ".txt"
        INFile10 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_A_E_O_" & txtFile & ".txt"
        INFile11 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_A_B_E_" & txtFile & ".txt"
        INFile12 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_A_B_O_" & txtFile & ".txt"
        INFile13 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_A_D_E_" & txtFile & ".txt"
        INFile14 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_A_D_O_" & txtFile & ".txt"
        INFile15 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_A_EXT_" & txtFile & ".txt"
        INFile16 = System.IO.File.CreateText(OutFile)
        OutFile = PostFile & "EXPO_A_CAC_" & txtFile & ".txt"
        INFile17 = System.IO.File.CreateText(OutFile)

        Dim arrINPUT_M_E_E(iAnzahl - 1) As File
        Dim arrINPUT_M_E_O(iAnzahl - 1) As File
        Dim arrINPUT_M_B_E(iAnzahl - 1) As File
        Dim arrINPUT_M_B_O(iAnzahl - 1) As File
        Dim arrINPUT_M_D_E(iAnzahl - 1) As File
        Dim arrINPUT_M_D_O(iAnzahl - 1) As File
        Dim arrINPUT_M_EXT(iAnzahl - 1) As File
        Dim arrINPUT_M_CAI(iAnzahl - 1) As File
        Dim arrINPUT_M_CAC(iAnzahl - 1) As File
        Dim arrINPUT_A_E_E(iAnzahl - 1) As File
        Dim arrINPUT_A_E_O(iAnzahl - 1) As File
        Dim arrINPUT_A_B_E(iAnzahl - 1) As File
        Dim arrINPUT_A_B_O(iAnzahl - 1) As File
        Dim arrINPUT_A_D_E(iAnzahl - 1) As File
        Dim arrINPUT_A_D_O(iAnzahl - 1) As File
        Dim arrINPUT_A_EXT(iAnzahl - 1) As File
        Dim arrINPUT_A_CAC(iAnzahl - 1) As File

        For i = 0 To iAnzahl - 1
            'eclaboussure et ensoleillement
            arrINPUT_M_E_E(i).HR = arrMatrice(i).HR_eclaboussures
            arrINPUT_M_E_E(i).Sel = arrMatrice(i).salage1
            arrINPUT_M_E_E(i).Tsurf = arrMatrice(i).Ts
            'eclaboussure et ombr�e
            arrINPUT_M_E_O(i).HR = arrMatrice(i).HR_eclaboussures
            arrINPUT_M_E_O(i).Sel = arrMatrice(i).salage1
            arrINPUT_M_E_O(i).Tsurf = arrMatrice(i).T
            'brouillard et ensoleillement
            arrINPUT_M_B_E(i).HR = arrMatrice(i).HR_brouillard
            arrINPUT_M_B_E(i).Sel = arrMatrice(i).salage1
            arrINPUT_M_B_E(i).Tsurf = arrMatrice(i).Ts
            'brouillard et ombr�e
            arrINPUT_M_B_O(i).HR = arrMatrice(i).HR_brouillard
            arrINPUT_M_B_O(i).Sel = arrMatrice(i).salage1
            arrINPUT_M_B_O(i).Tsurf = arrMatrice(i).T
            'direct et ensoleillement
            arrINPUT_M_D_E(i).HR = arrMatrice(i).HR_direct
            arrINPUT_M_D_E(i).Sel = arrMatrice(i).salage1
            arrINPUT_M_D_E(i).Tsurf = arrMatrice(i).Ts
            'direct et ombr�e
            arrINPUT_M_D_O(i).HR = arrMatrice(i).HR_direct
            arrINPUT_M_D_O(i).Sel = arrMatrice(i).salage1
            arrINPUT_M_D_O(i).Tsurf = arrMatrice(i).T
            'ext�rieur et � l'abris des intemp�ries
            arrINPUT_M_EXT(i).HR = arrMatrice(i).HR_ext
            arrINPUT_M_EXT(i).Sel = arrMatrice(i).salage1
            arrINPUT_M_EXT(i).Tsurf = arrMatrice(i).Text
            'int�rieur du caisson et sans sel
            arrINPUT_M_CAI(i).HR = arrMatrice(i).HR_caisson
            arrINPUT_M_CAI(i).Sel = 0
            arrINPUT_M_CAI(i).Tsurf = arrMatrice(i).Tcaisson
            'int�rieur du caisson et avec pr�sence de sel
            arrINPUT_M_CAC(i).HR = arrMatrice(i).HR_caisson
            arrINPUT_M_CAC(i).Sel = arrMatrice(i).salage1
            arrINPUT_M_CAC(i).Tsurf = arrMatrice(i).Tcaisson

            'eclaboussure et ensoleillement
            arrINPUT_A_E_E(i).HR = arrMatrice(i).HR_eclaboussures
            arrINPUT_A_E_E(i).Sel = arrMatrice(i).salage2
            arrINPUT_A_E_E(i).Tsurf = arrMatrice(i).Ts
            'eclaboussure et ombr�e
            arrINPUT_A_E_O(i).HR = arrMatrice(i).HR_eclaboussures
            arrINPUT_A_E_O(i).Sel = arrMatrice(i).salage2
            arrINPUT_A_E_O(i).Tsurf = arrMatrice(i).T
            'brouillard et ensoleillement
            arrINPUT_A_B_E(i).HR = arrMatrice(i).HR_brouillard
            arrINPUT_A_B_E(i).Sel = arrMatrice(i).salage2
            arrINPUT_A_B_E(i).Tsurf = arrMatrice(i).Ts
            'brouillard et ombr�e
            arrINPUT_A_B_O(i).HR = arrMatrice(i).HR_brouillard
            arrINPUT_A_B_O(i).Sel = arrMatrice(i).salage2
            arrINPUT_A_B_O(i).Tsurf = arrMatrice(i).T
            'direct et ensoleillement
            arrINPUT_A_D_E(i).HR = arrMatrice(i).HR_direct
            arrINPUT_A_D_E(i).Sel = arrMatrice(i).salage2
            arrINPUT_A_D_E(i).Tsurf = arrMatrice(i).Ts
            'direct et ombr�e
            arrINPUT_A_D_O(i).HR = arrMatrice(i).HR_direct
            arrINPUT_A_D_O(i).Sel = arrMatrice(i).salage2
            arrINPUT_A_D_O(i).Tsurf = arrMatrice(i).T
            'ext�rieur et � l'abris des intemp�ries
            arrINPUT_A_EXT(i).HR = arrMatrice(i).HR_ext
            arrINPUT_A_EXT(i).Sel = arrMatrice(i).salage2
            arrINPUT_A_EXT(i).Tsurf = arrMatrice(i).Text
            'int�rieur du caisson et avec pr�sence de sel
            arrINPUT_A_CAC(i).HR = arrMatrice(i).HR_caisson
            arrINPUT_A_CAC(i).Sel = arrMatrice(i).salage2
            arrINPUT_A_CAC(i).Tsurf = arrMatrice(i).Tcaisson
        Next
        '�criture dans les fichiers
        INFile1.WriteLine(iAnzahl)
        INFile1.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile1.WriteLine(arrINPUT_M_E_E(i).HR & vbTab & vbTab & arrINPUT_M_E_E(i).Sel & vbTab & vbTab & arrINPUT_M_E_E(i).Tsurf, i)
        Next
        INFile1.Close()

        INFile2.WriteLine(iAnzahl)
        INFile2.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile2.WriteLine(arrINPUT_M_E_O(i).HR & vbTab & vbTab & arrINPUT_M_E_O(i).Sel & vbTab & vbTab & arrINPUT_M_E_O(i).Tsurf, i)
        Next
        INFile2.Close()

        INFile3.WriteLine(iAnzahl)
        INFile3.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile3.WriteLine(arrINPUT_M_B_E(i).HR & vbTab & vbTab & arrINPUT_M_B_E(i).Sel & vbTab & vbTab & arrINPUT_M_B_E(i).Tsurf, i)
        Next
        INFile3.Close()

        INFile4.WriteLine(iAnzahl)
        INFile4.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile4.WriteLine(arrINPUT_M_B_O(i).HR & vbTab & vbTab & arrINPUT_M_B_O(i).Sel & vbTab & vbTab & arrINPUT_M_B_O(i).Tsurf, i)
        Next
        INFile4.Close()

        INFile5.WriteLine(iAnzahl)
        INFile5.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile5.WriteLine(arrINPUT_M_D_E(i).HR & vbTab & vbTab & arrINPUT_M_D_E(i).Sel & vbTab & vbTab & arrINPUT_M_D_E(i).Tsurf, i)
        Next
        INFile5.Close()

        INFile6.WriteLine(iAnzahl)
        INFile6.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile6.WriteLine(arrINPUT_M_D_O(i).HR & vbTab & vbTab & arrINPUT_M_D_O(i).Sel & vbTab & vbTab & arrINPUT_M_D_O(i).Tsurf, i)
        Next
        INFile6.Close()

        INFile7.WriteLine(iAnzahl)
        INFile7.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile7.WriteLine(arrINPUT_M_EXT(i).HR & vbTab & vbTab & arrINPUT_M_EXT(i).Sel & vbTab & vbTab & arrINPUT_M_EXT(i).Tsurf, i)
        Next
        INFile7.Close()

        INFile8.WriteLine(iAnzahl)
        INFile8.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile8.WriteLine(arrINPUT_M_CAI(i).HR & vbTab & vbTab & arrINPUT_M_CAI(i).Sel & vbTab & vbTab & arrINPUT_M_CAI(i).Tsurf, i)
        Next
        INFile8.Close()

        INFile9.WriteLine(iAnzahl)
        INFile9.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile9.WriteLine(arrINPUT_M_CAC(i).HR & vbTab & vbTab & arrINPUT_M_CAC(i).Sel & vbTab & vbTab & arrINPUT_M_CAC(i).Tsurf, i)
        Next
        INFile9.Close()

        INFile10.WriteLine(iAnzahl)
        INFile10.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile10.WriteLine(arrINPUT_A_E_E(i).HR & vbTab & vbTab & arrINPUT_A_E_E(i).Sel & vbTab & vbTab & arrINPUT_A_E_E(i).Tsurf, i)
        Next
        INFile10.Close()

        INFile11.WriteLine(iAnzahl)
        INFile11.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile11.WriteLine(arrINPUT_A_E_O(i).HR & vbTab & vbTab & arrINPUT_A_E_O(i).Sel & vbTab & vbTab & arrINPUT_A_E_O(i).Tsurf, i)
        Next
        INFile11.Close()

        INFile12.WriteLine(iAnzahl)
        INFile12.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile12.WriteLine(arrINPUT_A_B_E(i).HR & vbTab & vbTab & arrINPUT_A_B_E(i).Sel & vbTab & vbTab & arrINPUT_A_B_E(i).Tsurf, i)
        Next
        INFile12.Close()

        INFile13.WriteLine(iAnzahl)
        INFile13.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile13.WriteLine(arrINPUT_A_B_O(i).HR & vbTab & vbTab & arrINPUT_A_B_O(i).Sel & vbTab & vbTab & arrINPUT_A_B_O(i).Tsurf, i)
        Next
        INFile13.Close()

        INFile14.WriteLine(iAnzahl)
        INFile14.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile14.WriteLine(arrINPUT_A_D_E(i).HR & vbTab & vbTab & arrINPUT_A_D_E(i).Sel & vbTab & vbTab & arrINPUT_A_D_E(i).Tsurf, i)
        Next
        INFile14.Close()

        INFile15.WriteLine(iAnzahl)
        INFile15.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile15.WriteLine(arrINPUT_A_D_O(i).HR & vbTab & vbTab & arrINPUT_A_D_O(i).Sel & vbTab & vbTab & arrINPUT_A_D_O(i).Tsurf, i)
        Next
        INFile15.Close()

        INFile16.WriteLine(iAnzahl)
        INFile16.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile16.WriteLine(arrINPUT_A_EXT(i).HR & vbTab & vbTab & arrINPUT_A_EXT(i).Sel & vbTab & vbTab & arrINPUT_A_EXT(i).Tsurf, i)
        Next
        INFile16.Close()

        INFile17.WriteLine(iAnzahl)
        INFile17.WriteLine("3600")
        For i = 0 To iAnzahl - 1
            INFile17.WriteLine(arrINPUT_A_CAC(i).HR & vbTab & vbTab & arrINPUT_A_CAC(i).Sel & vbTab & vbTab & arrINPUT_A_CAC(i).Tsurf, i)
        Next
        INFile17.Close()
        frmTempSeuil.Label12.Text = ""
        frmTempSeuil.Label3.Text = ""
        frmTempSeuil.Label6.Text = ""
        frmTempSeuil.Label22.Text = ""
        frmTempSeuil.Label74.Text = ""
        frmTempSeuil.Label76.Text = ""
        frmTempSeuil.Label66.Text = ""
b:
    End Sub

    Public Sub WCal()
        Dim NbrAns As Integer '[-]
        Dim i As Integer '[-]
        Dim DureeIntrvent As Integer '[h]
        Dim nbrInt1 As Long = 0
        Dim nbrInt2 As Long = 0
        Dim Tseuil1 As Single = -9  '�C
        Dim Tseuil2 As Single = -9  '�C
        Dim HRseuil As Single = 0
        Dim Nint1 As Long = 0
        Dim Nint2 As Long = 0
        Dim Dint As Short = 0
        Dim PluieOld As Boolean = False
        Dim QNa As Single = 0
        Dim EpNa As Single = 0
        Dim Feau As Single
        Dim Na As String

        CalNeige()

        DureeIntrvent = frmTempSeuil.NumericUpDown2.Value
        nbrInt1 = CInt(frmTempSeuil.Label3.Text / frmTempSeuil.NumericUpDown1.Value)
        nbrInt2 = CInt(frmTempSeuil.Label74.Text / (frmTempSeuil.NumericUpDown24.Value * frmTempSeuil.NumericUpDown25.Value))
        frmTempSeuil.Label6.Text = nbrInt1
        frmTempSeuil.Label76.Text = nbrInt2
        NbrAns = CInt(frmTempSeuil.Label12.Text)
        nbrInt1 = nbrInt1 * NbrAns
        nbrInt2 = nbrInt2 * NbrAns
        HRseuil = frmTempSeuil.NumericUpDown3.Value
        Do While Nint1 < nbrInt1
            Nint1 = 0
            Dint = 0
            For i = 0 To iAnzahl - 1
                If Dint <> 0 Then Dint = Dint + 1
                If Dint >= DureeIntrvent Then Dint = 0
                If Dint = 0 And arrDaten(i).moy6 / 10 < Tseuil1 And (arrDaten(i).moy13 / 10 >= HRseuil Or arrDaten(i).moy17 / 10 > 0) Then
                    Nint1 = Nint1 + 1
                    Dint = Dint + 1
                End If
            Next
            Tseuil1 = Tseuil1 + 0.1
        Loop
        frmTempSeuil.Label22.Text = CInt(Tseuil1 * 10) / 10
        Do While Nint2 < nbrInt2
            Nint2 = 0
            Dint = 0
            Na = 0
            EpNa = frmTempSeuil.NumericUpDown23.Text / 100
            QNa = frmTempSeuil.NumericUpDown24.Text * frmTempSeuil.NumericUpDown25.Text
            PluieOld = False
            Feau = frmTempSeuil.NumericUpDown5.Text
            For i = 0 To iAnzahl - 1
                If Dint <> 0 Then Dint = Dint + 1
                If Dint = 0 And arrDaten(i).moy6 / 10 < Tseuil2 And (arrDaten(i).moy13 / 10 >= HRseuil Or arrDaten(i).moy17 / 10 > 0) Then
                    If arrDaten(i).moy17 / 10 = 0 Then
                        Na = EpNa
                    Else
                        If PluieOld = False Then
                            Na = QNa / (1000 * arrDaten(i).moy17 / 10)
                        Else
                            If i <> 0 Then Na = (Na * 1000 * Feau + QNa) / ((Feau + arrDaten(i).moy17 / 10) * 1000)
                        End If
                    End If
                    Dint = Dint + 1
                    Nint2 = Nint2 + 1
                End If
                If arrDaten(i).moy17 / 10 <> 0 Then
                    PluieOld = True
                Else
                    PluieOld = False
                End If
                If Dint <> 1 Then
                    If PluieOld = True Then
                        If i > 0 Then Na = Na * 1000 * Feau / ((Feau + arrDaten(i).moy17 / 10) * 1000)
                    End If
                    If i = 0 Then Na = frmTempSeuil.NumericUpDown24.Value * frmTempSeuil.NumericUpDown25.Text / 100
                    If Na <= 0.1 * EpNa Then Dint = 0
                End If
                If Na > EpNa Then Na = EpNa
            Next
            Tseuil2 = Tseuil2 + 0.1
        Loop
        frmTempSeuil.Label66.Text = CInt(Tseuil2 * 10) / 10
        frmTempSeuil.Button2.Show()
    End Sub

    Private Sub CalNeige()
        Dim i As Integer '[-]
        Dim SeuilNeige As Single '�C

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'r�partition de la neige
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim k As Integer
        Dim roh As Single 'densit� de neige [kg/m3]
        Dim cum As Single = 0
        SeuilNeige = 4
        If CasInput = 1 Then 'avec donn�es de neige moy80
            For i = 0 To iAnzahl - 1
                If arrDaten(i).moy80 = 32767 Or arrDaten(i).moy80 = 0 Then
                    k = k + 1
                    If k > 15 Then
                        k = 15
                    End If
                End If
                If arrDaten(i).moy80 <> 0 And arrDaten(i).moy80 <> 32767 Then 'il neige
                    For k = 0 To k - 1
                        If arrDaten(i - k).moy6 / 10 >= SeuilNeige Then
                            arrDaten(i - k).neige = 0
                        Else
                            If (arrDaten(i - k).moy6 / 10) <= -1 Then
                                roh = 3 * arrDaten(i - k).moy6 / 10 + 110
                            Else
                                roh = 23 * arrDaten(i - k).moy6 / 10 + 130
                            End If
                            arrDaten(i - k).neige = (arrDaten(i - k).moy17 / 10) * 1000 / roh  '1000 densit� de l'eau=cte
                        End If
                    Next k
                End If
            Next i
        Else 'sans donn�es de neige moy80
            For i = 0 To iAnzahl - 1
                If arrDaten(i).moy6 / 10 >= SeuilNeige Then
                    arrDaten(i).neige = 0
                Else
                    If (arrDaten(i).moy6 / 10) <= -1 Then
                        roh = 3 * arrDaten(i).moy6 / 10 + 110
                    Else
                        roh = 23 * arrDaten(i).moy6 / 10 + 130
                    End If
                    arrDaten(i).neige = (arrDaten(i).moy17 / 10) * 1000 / roh    '1000 densit� de l'eau=cte
                End If
                cum = cum + arrDaten(i).neige
                If cum < 2 Then
                    arrDaten(i).neige = 0
                End If
                If arrDaten(i).moy80 = 0 Then
                    cum = 0
                End If
            Next i
        End If
    End Sub

    Private Sub AttenBruit(ByRef A As Single, ByRef B As Single, ByRef C As Single, ByRef D As Single, ByRef tempInput() As Single, ByRef tempOutput() As Single, ByRef tlim As Single)
        Dim dT1 As Single
        Dim dT2 As Single
        Dim bT1 As Single
        Dim bT2 As Single
        Dim T1 As Single
        Dim T2 As Single
        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim l As Integer
        Dim PentePos As Boolean

        For i = 0 To iAnzahl - 1
            k = l
            For j = k To iAnzahl - 3 'trouve le min et le max temp�rature 
                dT1 = tempInput(j + 1) - tempInput(j)
                dT2 = tempInput(j + 2) - tempInput(j + 1)
                If j = k Then bT1 = tempInput(j)
                If dT1 > 0 And j = k Then
                    PentePos = True
                ElseIf dT1 < 0 And j = k Then
                    PentePos = False
                ElseIf dT1 = 0 And j = k Then
                    bT1 = tempInput(j + 1)
                End If
                If PentePos = True And dT2 < 0 Then
                    bT2 = tempInput(j + 1)
                    Exit For
                ElseIf PentePos = False And dT2 > 0 Then
                    bT2 = tempInput(j + 1)
                    Exit For
                End If
            Next
            bT1 = A * (bT2 - bT1) / B + bT1  'calcul de la moyenne
            For l = k To j
                dT1 = tempInput(l + 1) - tempInput(l)
                If dT1 <> 0 Then
                    dT2 = C / (D * dT1)
                Else
                    dT2 = 0
                End If
                If System.Math.Abs(dT2) > tlim Then dT2 = 0
                dT1 = dT1 * dT2
                If l = 0 Then
                    T1 = tempInput(l) - dT1
                    T2 = tempInput(l) + dT1
                Else
                    T1 = tempOutput(l - 1) - dT1
                    T2 = tempOutput(l - 1) + dT1
                End If
                If System.Math.Abs(bT1 - T2) < System.Math.Abs(bT1 - T1) Then
                    tempOutput(l) = T2
                Else
                    tempOutput(l) = T1
                End If
            Next
            If l = iAnzahl - 1 Then Exit For
        Next

    End Sub


End Module

