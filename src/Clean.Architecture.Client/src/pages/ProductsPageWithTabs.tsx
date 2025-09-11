import { useState } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import ProductManagement from "@/components/ProductManagement";
import ProductManagementTable from "@/components/ProductManagementTable";

export function ProductsPage() {
  const [activeTab, setActiveTab] = useState("table");

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Products</h1>
          <p className="text-muted-foreground">
            Manage your product catalog and inventory
          </p>
        </div>
      </div>

      <Tabs
        value={activeTab}
        onValueChange={setActiveTab}
        className="space-y-4"
      >
        <TabsList>
          <TabsTrigger value="table">Table View</TabsTrigger>
          <TabsTrigger value="cards">Card View</TabsTrigger>
        </TabsList>

        <TabsContent value="table" className="space-y-4">
          <ProductManagementTable />
        </TabsContent>

        <TabsContent value="cards" className="space-y-4">
          <ProductManagement />
        </TabsContent>
      </Tabs>
    </div>
  );
}
