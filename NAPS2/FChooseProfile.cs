/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan;
using Ninject;

namespace NAPS2
{
    public partial class FChooseProfile : Form
    {
        private readonly IProfileManager profileManager;
        private readonly IScanPerformer scanPerformer;
        private readonly IScanReceiver scanReceiver;

        public FChooseProfile(IProfileManager profileManager, IScanPerformer scanPerformer, IScanReceiver scanReceiver)
        {
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
            this.scanReceiver = scanReceiver;
            InitializeComponent();
        }

        private ScanSettings SelectedProfile
        {
            get
            {
                if (lvProfiles.SelectedIndices.Count == 1)
                {
                    return profileManager.Profiles[lvProfiles.SelectedIndices[0]];
                }
                return null;
            }
        }

        private void FChooseProfile_Load(object sender, EventArgs e)
        {
            lvProfiles.LargeImageList = ilProfileIcons.IconsList;
            UpdateProfiles();
        }

        private void UpdateProfiles()
        {
            lvProfiles.Items.Clear();
            foreach (ScanSettings profile in profileManager.Profiles)
            {
                lvProfiles.Items.Add(profile.DisplayName, profile.IconID);
                if (profile.IsDefault)
                {
                    lvProfiles.Items[lvProfiles.Items.Count - 1].Selected = true;
                }
            }
            if (profileManager.Profiles.Count == 1)
            {
                lvProfiles.Items[0].Selected = true;
            }
        }

        private void lvProfiles_ItemActivate(object sender, EventArgs e)
        {
            if (SelectedProfile != null)
            {
                PerformScan();
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            PerformScan();
        }

        private void PerformScan()
        {
            if (profileManager.Profiles.Count == 0)
            {
                var editSettingsForm = KernelManager.Kernel.Get<FEditScanSettings>();
                editSettingsForm.ScanSettings = new ExtendedScanSettings();
                editSettingsForm.ShowDialog();
                if (editSettingsForm.Result)
                {
                    profileManager.Profiles.Add(editSettingsForm.ScanSettings);
                    profileManager.Save();
                    UpdateProfiles();
                    lvProfiles.SelectedIndices.Add(0);
                }
            }
            if (SelectedProfile == null)
            {
                MessageBox.Show("Select a profile before clicking Scan.", "Choose Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            profileManager.SetDefault(SelectedProfile);
            profileManager.Save();
            scanPerformer.PerformScan(SelectedProfile, this, scanReceiver);
        }

        private void btnProfiles_Click(object sender, EventArgs e)
        {
            KernelManager.Kernel.Get<FManageProfiles>().ShowDialog();
            UpdateProfiles();
        }
    }
}
