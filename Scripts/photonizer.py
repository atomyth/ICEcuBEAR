import csv
from scipy.interpolate import interp1d
import pandas as pd
import numpy as np
from scipy.spatial.distance import *
nGrid=250
maxVertices=500000
nSelect=50
maxPhotons=7500
splitVrtc=32000
master=pd.read_csv('rmx/hololens_icebear_hydrangea_photonpath_masterindex.csv')
master[master.t_f-master.t_i==(master.t_f-master.t_i).max()]
master=master[((master.t_f-master.t_i)>5)] #filter away photons with too short a life
tgrid=np.linspace(master.t_i.min()+0.1, master.t_f.max()-0.1, nGrid)
photdata={}
print "Preparing photdata"
for a in master.id:
    try:
        data=pd.read_csv('rmx/photons/%06d.csv'%(a,),names=["t","x","y","z"])
        photdata[a]=data
    except:
        pass
photinterp={}
print "Preparing interpolations"
for pid in photdata:
    dt=photdata[pid]
    #print dt[["x","y","z"]].as_matrix().T.shape
    photinterp[pid]=interp1d(dt.t.values,dt[["x","y","z"]].as_matrix().T)
maxsc=master.scattering.max()
master=master[master.id.isin(photdata.keys())].copy()
def normlz(npa):
    return npa/npa.sum()
def timepoint_choose(timepoint,chosen, mdata,nselect,reject=False,maxSctr=None,maxPunish=0.5):
    includedPhotons=mdata[(mdata.t_i<=timepoint) &  (master.t_f>=timepoint)]
    left=includedPhotons[~includedPhotons.id.isin(chosen)]
    choice=includedPhotons[includedPhotons.id.isin(chosen)]
    if maxSctr is None: maxSctr=mdata.scattering.max()
    if len(left)==0: return mdata[mdata.id.isin(chosen)].scattering.values.sum()
    if len(choice)==0:
        if reject:
            return 0 #don't?
        else:    
            chosen.append(np.random.choice(left.id.values,1,p=normlz(1.0-float(maxPunish)+float(maxPunish)*(maxSctr-left.scattering.values)/maxSctr))[0])
            return mdata[mdata.id==chosen[0]].scattering.values[0]
    if nselect>len(left): nselect=len(left)
    seld=np.random.choice(left.id.values,nselect,p=normlz(1.0-float(maxPunish)+float(maxPunish)*(maxSctr-left.scattering.values)/maxSctr))
    chPoints=np.zeros((len(chosen),3))
    sPoints=np.zeros((len(seld),3))
    cnt=0
    for pid in choice.id.values:
        try:
         chPoints[cnt,:]=photinterp[pid](timepoint)
        except:
         print "Err occured "
         print timepoint
         return mdata[mdata.id.isin(chosen)].scattering.values.sum()        
        cnt=cnt+1
    cnt=0
    for pid in seld:
        try:
         sPoints[cnt,:]=photinterp[pid](timepoint)
        except:
          return mdata[mdata.id.isin(chosen)].scattering.values.sum()        
        cnt=cnt+1
    dmatr=cdist(chPoints,sPoints)
    if reject:
        dvec=dmatr.min(0)
        mdata.drop(mdata[mdata.id==seld[np.argmin(dvec)]].index,inplace=True)
        return mdata[mdata.id.isin(chosen)].scattering.values.sum()
    else:
        dvec=dmatr.max(0)
        iap=seld[np.argmax(dvec)]
        chosen.append(iap)
        return mdata[mdata.id.isin(chosen)].scattering.values.sum()
nvert=0
#chosenPhotons=[]
chosenPhotons=[12, 22, 24, 29, 44, 51, 55, 64, 84, 105, 106, 136, 156, 160, 165, 175, 182, 190, 196, 200, 261, 263, 269, 287, 293, 299, 307, 314, 315, 319, 360, 368, 373, 374, 387 ]

print len(master)
print "Collecting photons"
while nvert<maxVertices and len(master)>0 and len(chosenPhotons)<maxPhotons:
    for pt in reversed(tgrid):
        nvert=timepoint_choose(pt,chosenPhotons,master,nSelect)
        timepoint_choose(pt,chosenPhotons,master,nSelect,True)
        if nvert>=maxVertices or len(master)==0: break
    print "Vertices: %d Photons: %d" %(nvert,len(chosenPhotons))    
print len(master) 
chosenPhotons=chosenPhotons[:-1]
print "Collecting mesh bundles"
vrtc=0
bundle=0
res=None
for photon in chosenPhotons:
   pdat=photdata[photon]
   if(len(pdat)+vrtc>splitVrtc):
    vrtc=0
    bundle=bundle+1
   pdat["id"]=pd.Series([photon]*len(pdat)) 
   pdat["bundle"]=pd.Series([bundle]*len(pdat)) 
   vrtc=vrtc+len(pdat)
   if res is None:
      res=pdat.copy()
   else:
      res=res.append(pdat)   
res.to_csv("meshes_%d_%d.csv"%(nvert,len(chosenPhotons)))      