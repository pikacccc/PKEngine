#include "Test.h"

#define TEST_ENTITY_COMPONENTS 1

#if TEST_ENTITY_COMPONENTS
class engine_test : public test
{
public:
	bool initialize() override { return true; }
	void run() override {}
	void shutdown() override {}
};
#else
#error One of the tests need to be enabled
#endif

int main() {
	engine_test test{};

	if (test.initialize()) {
		test.run();
	}

	test.shutdown();
}